namespace Duck.Content;

public struct MaterialAsset : IMaterialAsset
{
    #region Properties

    public Guid Id { get; }
    public Uri Uri { get; }
    public AssetStatus Status { get; }

    #endregion

    #region Methods

    public MaterialAsset(Uri uri)
    {
        Id = Guid.NewGuid();
        Uri = uri;
        Status = AssetStatus.Unloaded;
    }

    #endregion
}

public struct Material : IMaterial
{
    public IMaterialAsset Asset { get; }

    public Material(IMaterialAsset asset)
    {
        Asset = asset;
    }
}
