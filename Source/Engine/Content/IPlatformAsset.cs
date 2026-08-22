using WeakEvent;

namespace Duck.Content;

public interface IPlatformAsset
{
    WeakEventSource<PlatformAssetReloadedEvent> Reloaded { get; }
}

public interface IPlatformAsset<TAssetType> : IPlatformAsset
    where TAssetType : class, IAsset
{
}

public interface IPlatformAssetCollection
{
}

public class PlatformAssetCollection<TAssetType, TPlatormType> : IPlatformAssetCollection
    where TAssetType : class, IAsset
    where TPlatormType : class, IPlatformAsset
{
    private Dictionary<AssetReference<TAssetType>, TPlatormType> _items = new();

    public void Add(in AssetReference<TAssetType> assetReference, TPlatormType platformAsset)
    {
        _items.Add(assetReference, platformAsset);
    }

    public bool TryGet(in AssetReference<TAssetType> assetReference, out TPlatormType? value)
    {

        if (!TryGetBase(assetReference, out var ret)) {
            value = null;
        } else {
            value = ret as TPlatormType;
        }

        return value != null;
    }

    public TPlatormType Get(in AssetReference<TAssetType> assetReference)
    {
        return _items[assetReference];
    }

    public void Remove(in AssetReference<TAssetType> assetReference)
    {
        _items.Remove(assetReference);
    }

    public IPlatformAsset GetBase(in AssetReference<TAssetType> assetReference)
    {
        return Get(assetReference);
    }

    public bool TryGetBase(in AssetReference<TAssetType> assetReference, out IPlatformAsset? value)
    {
        value = _items.GetValueOrDefault(assetReference);

        return value != null;
    }

    public bool Contains(in AssetReference<TAssetType> assetReference)
    {
        return _items.ContainsKey(assetReference);
    }
}

public class PlatformAssetReloadedEvent : EventArgs
{
}