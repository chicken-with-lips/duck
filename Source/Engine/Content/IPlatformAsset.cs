using WeakEvent;

namespace Duck.Content;

public interface IPlatformAsset
{
    public WeakEventSource<ReloadEvent> Reloaded { get; }
}

public interface IPlatformAsset<T> : IPlatformAsset
    where T : class, IAsset
{
}

public interface IPlatformAssetCollection
{
}

public interface IPlatformAssetCollection<T> : IPlatformAssetCollection
    where T : class, IAsset
{
    public bool Contains(AssetReference<T> assetReference);
    public void Add(AssetReference<T> assetReference, IPlatformAsset platformAsset);
    public IPlatformAsset GetBase(AssetReference<T> assetReference);
    public bool TryGetBase(AssetReference<T> assetReference, out IPlatformAsset? value);
}

public class PlatformAssetCollection<T, U> : IPlatformAssetCollection<T>
    where T : class, IAsset
    where U : class, IPlatformAsset
{
    private Dictionary<AssetReference<T>, U> _items = new();

    public void Add(AssetReference<T> assetReference, U platformAsset)
    {
        _items.Add(assetReference, platformAsset);
    }

    public void Add(AssetReference<T> assetReference, IPlatformAsset platformAsset)
    {
        _items.Add(assetReference, platformAsset as U);
    }

    public bool TryGet(AssetReference<T> assetReference, out U? value)
    {
        IPlatformAsset? ret;

        if (!TryGetBase(assetReference, out ret)) {
            value = null;
            return false;
        }

        value = ret as U;

        return true;
    }

    public U Get(AssetReference<T> assetReference)
    {
        return _items[assetReference] as U;
    }

    public void Remove(AssetReference<T> assetReference)
    {
        _items.Remove(assetReference);
    }

    public IPlatformAsset GetBase(AssetReference<T> assetReference)
    {
        return Get(assetReference);
    }

    public bool TryGetBase(AssetReference<T> assetReference, out IPlatformAsset? value)
    {
        if (!_items.ContainsKey(assetReference)) {
            value = null;
            return false;
        }

        value = _items[assetReference];

        return true;
    }

    public bool Contains(AssetReference<T> assetReference)
    {
        return _items.ContainsKey(assetReference);
    }
}

public class ReloadEvent : EventArgs
{
}
