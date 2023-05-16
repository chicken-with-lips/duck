using System.Diagnostics;
using Duck.Content;
using Duck.Renderer.Device;
using Duck.Renderer.Shaders;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL.ContentLoader;

internal class ShaderProgramLoader : IAssetLoader
{
    private readonly GL _api;
    private readonly IContentModule _contentModule;

    public ShaderProgramLoader(
        OpenGLGraphicsDevice graphicsDevice,
        IContentModule contentModule)
    {
        _api = graphicsDevice.API;
        _contentModule = contentModule;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is ShaderProgram;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not ShaderProgram programAsset) {
            throw new Exception("FIXME: errors");
        }

        var vertexShader = (OpenGLVertexShader)_contentModule.LoadImmediate(programAsset.VertexShader);
        var fragmentShader = (OpenGLFragmentShader)_contentModule.LoadImmediate(programAsset.FragmentShader);

        var programId = LoadProgram(vertexShader, fragmentShader);

        if (loadInto != null) {
            throw new Exception("FIXME: hot reload not supported");
        }

        var shaderProgram = new OpenGLShaderProgram(programId, vertexShader, fragmentShader);
        vertexShader.Reloaded.Subscribe(this, (object? sender, ReloadEvent ev) => OnShaderReloaded(shaderProgram, ev));
        fragmentShader.Reloaded.Subscribe(this, (object? sender, ReloadEvent ev) => OnShaderReloaded(shaderProgram, ev));

        return shaderProgram;
    }

    private uint LoadProgram(OpenGLVertexShader vertexShader, OpenGLFragmentShader fragmentShader)
    {
        var programId = _api.CreateProgram();

        _api.AttachShader(programId, vertexShader.ShaderId);
        OpenGLUtil.LogErrors(_api);

        _api.AttachShader(programId, fragmentShader.ShaderId);
        OpenGLUtil.LogErrors(_api);

        foreach (var attributeIndex in Enum.GetValues<VertexAttribute>()) {
            _api.BindAttribLocation(programId, (uint)attributeIndex, "in" + Enum.GetName(attributeIndex));
            OpenGLUtil.LogErrors(_api);
        }

        _api.LinkProgram(programId);
        _api.GetProgram(programId, GLEnum.LinkStatus, out var status);

        if (status == 0) {
            throw new Exception($"TODO: Error linking shader {_api.GetProgramInfoLog(programId)}");
        }

        return programId;
    }

    private void OnShaderReloaded(object? platformAsset, ReloadEvent e)
    {
        Debug.Assert(platformAsset is OpenGLShaderProgram);

        var shaderProgram = (OpenGLShaderProgram)platformAsset;
        var programId = LoadProgram(shaderProgram.VertexShader, shaderProgram.FragmentShader);

        _api.DeleteProgram(shaderProgram.ProgramId);

        shaderProgram.ProgramId = programId;
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is OpenGLShaderProgram programAsset) {
            _api.DeleteProgram(programAsset.ProgramId);
            OpenGLUtil.LogErrors(_api);
        }
    }
}
