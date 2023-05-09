namespace Duck.Content;

public interface IContentModule : IModule
{
    public IAssetDatabase Database { get; }
    public string ContentRootDirectory { get; set; }
    public bool ReloadChangedContent { get; set; }

    public IContentModule RegisterSourceAssetImporter(ISourceAssetImporter importer);

    public ISourceAssetImporter<TAsset>? FindSourceAssetImporter<TAsset>(string file)
        where TAsset : class, IAsset;

    public TAsset? Import<TAsset>(string file)
        where TAsset : class, IAsset;

    public IContentModule RegisterAssetLoader<T, U>(IAssetLoader loader)
        where T : class, IAsset
        where U : class, IPlatformAsset;

    public IAssetLoader? FindAssetLoader<T>(T asset, IAssetLoadContext context)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(IAssetReference<T> assetReference, IAssetLoadContext context, byte[]? fixmeData = null)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(IAssetReference<T> asset)
        where T : class, IAsset;

    bool IsLoaded<T>(IAssetReference<T> assetReference)
        where T : class, IAsset;
}

public interface IAssetReference<T>  where T : class, IAsset
{
    public Guid AssetId { get; }
}

public readonly struct SharedAssetReference<T> : IAssetReference<T>
    where T : class, IAsset
{
    public Guid AssetId { get; }

    public SharedAssetReference(Guid assetId)
    {
        AssetId = assetId;
    }
}

public readonly struct UniqueAssetReference<T> : IAssetReference<T>
    where T : class, IAsset
{
    public Guid AssetId { get; }
    public Guid UniqueId { get; }

    public UniqueAssetReference(Guid assetId)
    {
        AssetId = assetId;
        UniqueId = Guid.NewGuid();
    }
}

public interface IAssetLoadContext
{
}

public struct EmptyAssetLoadContext : IAssetLoadContext
{
    public static EmptyAssetLoadContext Default => new();
}
