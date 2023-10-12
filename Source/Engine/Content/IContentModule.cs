using Duck.Platform;

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

    public IPlatformAsset LoadImmediate<T>(AssetReference<T> assetReference, IAssetLoadContext context, byte[]? fixmeData = null)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(AssetReference<T> asset)
        where T : class, IAsset;

    bool IsLoaded<T>(AssetReference<T> assetReference)
        where T : class, IAsset;
}

public readonly struct AssetReference<T>
    where T : class, IAsset
{
    public Guid AssetId { get; init; }

    public Guid UniqueId { get; init; }
    public bool IsShared { get; init; }
    public bool IsUnique => !IsShared;

    public static AssetReference<T> Shared(Guid assetId)
    {
        return new AssetReference<T>() {
            AssetId = assetId,
            IsShared = true,
        };
    }

    public static AssetReference<T> Unique(Guid assetId)
    {
        return new AssetReference<T>() {
            AssetId = assetId,
            UniqueId = Guid.NewGuid(),
            IsShared = false,
        };
    }
}

public interface IAssetLoadContext
{
}

public struct EmptyAssetLoadContext : IAssetLoadContext
{
    public static EmptyAssetLoadContext Default => new();
}
