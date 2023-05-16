using Duck.Content;
using Duck.Renderer.Materials;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLMaterial : PlatformAssetBase<Material>
{
    public OpenGLShaderProgram ShaderProgram { get; }
    public Material Material { get; }
    public OpenGLTexture2D? DiffuseTexture { get; }

    public OpenGLMaterial(OpenGLShaderProgram shaderProgram, Material material, OpenGLTexture2D? diffuseTexture)
    {
        ShaderProgram = shaderProgram;
        Material = material;
        DiffuseTexture = diffuseTexture;
    }
}
