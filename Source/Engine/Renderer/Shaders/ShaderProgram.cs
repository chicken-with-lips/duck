using Duck.Content;

namespace Duck.Renderer.Shaders;

public class ShaderProgram : AssetBase<ShaderProgram>
{
    public AssetReference<VertexShader> VertexShader { get; }
    public AssetReference<FragmentShader> FragmentShader { get; }

    #region Methods

    public ShaderProgram(AssetImportData importData, AssetReference<VertexShader> vertexShader, AssetReference<FragmentShader> fragmentShader)
        : base(importData)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }

    #endregion
}
