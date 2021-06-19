namespace Duck.Content;

public struct Texture2DAsset : ITexture2DAsset
{
    #region Properties

    public Guid Id { get; }
    public Uri Uri { get; }
    public AssetStatus Status { get; }

    public int AnisotropyLevel { get; }
    public MinFilter MinFilter { get; }
    public MagFilter MagFilter { get; }
    public WrapMode WrapS { get; }
    public WrapMode WrapT { get; }
    public WrapMode WrapR { get; }

    #endregion

    #region Methods

    public Texture2DAsset(Uri uri)
    {
        Id = Guid.NewGuid();
        Uri = uri;
        Status = AssetStatus.Unloaded;

        AnisotropyLevel = 1;
        MinFilter = MinFilter.Nearest;
        MagFilter = MagFilter.Linear;
        WrapS = WrapMode.Repeat;
        WrapT = WrapMode.Repeat;
        WrapR = WrapMode.Repeat;
    }

    #endregion
}

public struct Texture2D : ITexture2D
{
    public ITexture2DAsset Asset { get; }

    public Texture2D(ITexture2DAsset asset)
    {
        Asset = asset;
    }
}
