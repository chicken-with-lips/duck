using Duck.Content;

namespace Duck.Graphics.Shaders;

public class ShaderProgram : AssetBase<ShaderProgram>
{
    public IAssetReference<VertexShader> VertexShader { get; }
    public IAssetReference<FragmentShader> FragmentShader { get; }

    #region Methods

    public ShaderProgram(AssetImportData importData, IAssetReference<VertexShader> vertexShader, IAssetReference<FragmentShader> fragmentShader)
        : base(importData)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }

    #endregion
}
