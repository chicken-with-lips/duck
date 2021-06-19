using Duck.Content;

namespace Duck.AssetManagement;

public struct MaterialInstanceAsset : IMaterialInstanceAsset
{
    #region Properties

    public Guid Id { get; }
    public Uri Uri { get; }
    public AssetStatus Status { get; }
    public MaterialParameterCollection Parameters { get; }

    public AssetReference<IMaterialAsset> MaterialAssetReference { get; }

    #endregion

    #region Methods

    public MaterialInstanceAsset(Uri uri, AssetReference<IMaterialAsset> materialAssetReference)
    {
        Id = Guid.NewGuid();
        Uri = uri;
        Status = AssetStatus.Unloaded;
        Parameters = new();
        MaterialAssetReference = materialAssetReference;
    }

    public void SetParameter(string name, AssetReference<ITexture2DAsset> texture)
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

public struct MaterialInstance : IMaterialInstance
{
    public IMaterialInstanceAsset Asset { get; }

    public MaterialInstance(IMaterialInstanceAsset asset)
    {
        Asset = asset;
    }
}
