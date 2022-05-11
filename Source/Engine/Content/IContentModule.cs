namespace Duck.Content;

public interface IContentModule : IModule
{
    public IAssetDatabase Database { get; }

    public IContentModule RegisterAssetLoader<T, U>(IAssetLoader loader)
        where T : class, IAsset
        where U : class, IPlatformAsset;

    public IAssetLoader? FindAssetLoader<T>(T asset)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(T asset)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(AssetReference<T> asset)
        where T : class, IAsset;

    bool IsLoaded<T>(AssetReference<T> assetReference)
        where T : class, IAsset;
}

public readonly struct AssetReference<T>
    where T : class, IAsset
{
    public readonly Guid Id;

    public AssetReference(Guid id)
    {
        Id = id;
    }
}
