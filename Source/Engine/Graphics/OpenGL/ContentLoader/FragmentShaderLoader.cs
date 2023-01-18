using System.Text;
using Duck.Content;
using Duck.Graphics.Shaders;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL.ContentLoader;

internal class FragmentShaderLoader : IAssetLoader
{
    private readonly OpenGLGraphicsDevice _graphicsDevice;

    public FragmentShaderLoader(OpenGLGraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is FragmentShader;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not FragmentShader shaderAsset) {
            throw new Exception("FIXME: errors");
        }

        var api = _graphicsDevice.API;
        var shaderId = api.CreateShader(ShaderType.FragmentShader);

        api.ShaderSource(shaderId, Encoding.UTF8.GetString(source));
        api.CompileShader(shaderId);

        var infoLog = api.GetShaderInfoLog(shaderId);

        // TODO: return default asset instead
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new ApplicationException($"FIXME: Error compiling fragment shader {infoLog}");
        }

        return new OpenGLFragmentShader(shaderId);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is OpenGLFragmentShader shaderAsset) {
            _graphicsDevice.API.DeleteShader(shaderAsset.ShaderId);
        }
    }
}
