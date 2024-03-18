using System.Text;
using Duck.Content;
using Duck.Graphics.Shaders;
using Duck.Logging;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL.ContentLoader;

internal class VertexShaderLoader : IAssetLoader
{
    public OpenGLShaderProgram? FallbackShader { get; set; }

    private readonly ILogger _logger;
    private readonly GL _api;

    public VertexShaderLoader(OpenGLGraphicsDevice graphicsDevice, ILogger logger)
    {
        _logger = logger;
        _api = graphicsDevice.API;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is VertexShader;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not VertexShader shaderAsset) {
            throw new Exception("FIXME: errors");
        }

        var shaderId = _api.CreateShader(ShaderType.VertexShader);
        var defaultShaderId = FallbackShader?.FragmentShader.ShaderId ?? 0;

        _api.ShaderSource(shaderId, Encoding.UTF8.GetString(source));
        _api.CompileShader(shaderId);

        var infoLog = _api.GetShaderInfoLog(shaderId);

        // TODO: return default asset instead
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            _logger.LogError("Error compiling fragment shader: {0}", infoLog);

            _api.DeleteShader(shaderId);

            shaderId = defaultShaderId;
        }

        if (loadInto != null && loadInto is OpenGLVertexShader existingVertexShader) {
            if (existingVertexShader.ShaderId != defaultShaderId) {
                _api.DeleteShader(existingVertexShader.ShaderId);
            }

            existingVertexShader.ShaderId = shaderId;

            return existingVertexShader;
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
