using System.Text;
using Duck.Content;
using Duck.Graphics.Shaders;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL.ContentLoader;

public class VertexShaderLoader : IAssetLoader
{
    private readonly GL _api;

    public VertexShaderLoader(OpenGLGraphicsDevice graphicsDevice)
    {
        _api = graphicsDevice.API;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is VertexShader;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not VertexShader shaderAsset) {
            throw new Exception("FIXME: errors");
        }

        var shaderId = _api.CreateShader(ShaderType.VertexShader);
        _api.ShaderSource(shaderId, Encoding.UTF8.GetString(source));
        _api.CompileShader(shaderId);

        var infoLog = _api.GetShaderInfoLog(shaderId);

        // TODO: return default asset instead
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new ApplicationException($"FIXME: Error compiling vertex shader {infoLog}");
        }

        return new OpenGLVertexShader(shaderId);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is OpenGLVertexShader shaderAsset) {
            _api.DeleteShader(shaderAsset.ShaderId);
        }
    }
}
