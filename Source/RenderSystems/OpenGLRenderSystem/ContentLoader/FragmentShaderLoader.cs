using System.Text;
using Duck.Content;
using Duck.Graphics.Shaders;
using Duck.Logging;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL.ContentLoader;

internal class FragmentShaderLoader : IAssetLoader
{
    public OpenGLShaderProgram? FallbackShader { get; set; }

    private readonly OpenGLGraphicsDevice _graphicsDevice;
    private readonly ILogger _logger;

    public FragmentShaderLoader(OpenGLGraphicsDevice graphicsDevice, ILogger logger)
    {
        _graphicsDevice = graphicsDevice;
        _logger = logger;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is FragmentShader;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not FragmentShader shaderAsset) {
            throw new Exception("FIXME: errors");
        }

        var api = _graphicsDevice.API;
        var shaderId = api.CreateShader(ShaderType.FragmentShader);
        var defaultShaderId = FallbackShader?.FragmentShader.ShaderId ?? 0;

        api.ShaderSource(shaderId, Encoding.UTF8.GetString(source));
        api.CompileShader(shaderId);

        var infoLog = api.GetShaderInfoLog(shaderId);

        // TODO: return default asset instead
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            _logger.LogError("Error compiling fragment shader: {0}", infoLog);

            api.DeleteShader(shaderId);

            shaderId = defaultShaderId;
        }

        if (loadInto != null && loadInto is OpenGLFragmentShader existingFragmentShader) {
            if (existingFragmentShader.ShaderId != defaultShaderId) {
                api.DeleteShader(existingFragmentShader.ShaderId);
            }

            existingFragmentShader.ShaderId = shaderId;

            return existingFragmentShader;
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
