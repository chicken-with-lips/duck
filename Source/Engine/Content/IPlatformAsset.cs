namespace Duck.Content;

public interface IPlatformAsset
{
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
    public bool Contains(IAssetReference<T> assetReference);
    public void Add(IAssetReference<T> assetReference, IPlatformAsset platformAsset);
    public IPlatformAsset GetBase(IAssetReference<T> assetReference);
    public bool TryGetBase(IAssetReference<T> assetReference, out IPlatformAsset? value);
}

public class PlatformAssetCollection<T, U> : IPlatformAssetCollection<T>
    where T : class, IAsset
    where U : class, IPlatformAsset
{
    private Dictionary<IAssetReference<T>, U> _items = new();

    public void Add(IAssetReference<T> assetReference, U platformAsset)
    {
        _items.Add(assetReference, platformAsset);
    }

    public void Add(IAssetReference<T> assetReference, IPlatformAsset platformAsset)
    {
        _items.Add(assetReference, platformAsset as U);
    }

    public bool TryGet(IAssetReference<T> assetReference, out U? value)
    {
        IPlatformAsset? ret;

        if (!TryGetBase(assetReference, out ret)) {
            value = null;
            return false;
        }

        value = ret as U;

        return true;
    }

    public U Get(IAssetReference<T> assetReference)
    {
        return _items[assetReference] as U;
    }

    public void Remove(IAssetReference<T> assetReference)
    {
        _items.Remove(assetReference);
    }

    public IPlatformAsset GetBase(IAssetReference<T> assetReference)
    {
        return Get(assetReference);
    }

    public bool TryGetBase(IAssetReference<T> assetReference, out IPlatformAsset? value)
    {
        if (!_items.ContainsKey(assetReference)) {
            value = null;
            return false;
        }

        value = _items[assetReference];

        return true;
    }

    public bool Contains(IAssetReference<T> assetReference)
    {
        return _items.ContainsKey(assetReference);
    }
}
