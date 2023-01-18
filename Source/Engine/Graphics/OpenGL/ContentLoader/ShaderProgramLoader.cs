using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL.ContentLoader;

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

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not ShaderProgram programAsset) {
            throw new Exception("FIXME: errors");
        }

        var vertexShader = _contentModule.LoadImmediate(programAsset.VertexShader);
        var fragmentShader = _contentModule.LoadImmediate(programAsset.FragmentShader);

        var programId = _api.CreateProgram();

        _api.AttachShader(programId, ((OpenGLVertexShader)vertexShader).ShaderId);
        OpenGLUtil.LogErrors(_api);

        _api.AttachShader(programId, ((OpenGLFragmentShader)fragmentShader).ShaderId);
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

        return new OpenGLShaderProgram(programId);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is OpenGLShaderProgram programAsset) {
            _api.DeleteProgram(programAsset.ProgramId);
            OpenGLUtil.LogErrors(_api);
        }
    }
}
