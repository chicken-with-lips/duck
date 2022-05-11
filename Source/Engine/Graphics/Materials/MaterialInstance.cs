using Duck.Content;
using Duck.Graphics.Textures;

namespace Duck.Graphics.Materials;

public class MaterialInstance : AssetBase<MaterialInstance>
{
    #region Properties

    public MaterialParameterCollection Parameters { get; }
    public AssetReference<Material> MaterialAssetReference { get; }

    #endregion

    #region Methods

    public MaterialInstance(AssetImportData importData, AssetReference<Material> materialAssetReference)
        : base(importData)
    {
        Parameters = new();
        MaterialAssetReference = materialAssetReference;
    }

    public void SetParameter(string name, AssetReference<Texture2D> texture)
    {
        Parameters.SetParameter(name, texture);
    }

    // public void SetParameter(string name, RgbType rgbType, Color color)
    // {
    //     Parameters.SetParameter(name, rgbType, color);
    // }
    //
    // public void SetParameter(string name, RgbaType rgbaType, Color color)
    // {
    //     Parameters.SetParameter(name, rgbaType, color);
    // }

    #endregion
}
