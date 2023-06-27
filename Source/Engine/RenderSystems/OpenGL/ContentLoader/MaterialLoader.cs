using Duck.Content;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;

namespace Duck.RenderSystems.OpenGL.ContentLoader;

internal class MaterialLoader : IAssetLoader
{
    private readonly IContentModule _contentModule;

    public MaterialLoader(IContentModule contentModule)
    {
        _contentModule = contentModule;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is Material;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not Material materialAsset) {
            throw new Exception("FIXME: errors");
        }

        if (null == materialAsset.Shader) {
            throw new Exception("FIXME: shader is null");
        }

        var shaderProgram = (OpenGLShaderProgram)_contentModule.LoadImmediate(materialAsset.Shader.Value);

        OpenGLTexture2D? diffuseTexture = null;

        if (null != materialAsset.DiffuseTexture) {
            diffuseTexture = (OpenGLTexture2D)_contentModule.LoadImmediate(materialAsset.DiffuseTexture.Value);
        }

        if (loadInto != null) {
            throw new Exception("FIXME: Reloading materials not supported");
        }

        return new OpenGLMaterial(shaderProgram, materialAsset, diffuseTexture);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        throw new Exception("FIXME: cannot unload");
    }
}
