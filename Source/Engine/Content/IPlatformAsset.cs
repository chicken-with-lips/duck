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
    public bool Contains(T key);
    public void Add(T key, IPlatformAsset platformAsset);
    public IPlatformAsset GetBase(T key);
    public bool TryGetBase(T key, out IPlatformAsset? value);
}

public class PlatformAssetCollection<T, U> : IPlatformAssetCollection<T>
    where T : class, IAsset
    where U : class, IPlatformAsset
{
    private Dictionary<T, U> _items = new();

    public void Add(T asset, U platformAsset)
    {
        _items.Add(asset, platformAsset);
    }

    public void Add(T key, IPlatformAsset platformAsset)
    {
        _items.Add(key, platformAsset as U);
    }

    public bool TryGet(T key, out U? value)
    {
        IPlatformAsset? ret;

        if (!TryGetBase(key, out ret)) {
            value = null;
            return false;
        }

        value = ret as U;

        return true;
    }

    public U Get(T key)
    {
        return _items[key] as U;
    }

    public void Remove(T key)
    {
        _items.Remove(key);
    }

    public IPlatformAsset GetBase(T key)
    {
        return Get(key);
    }

    public bool TryGetBase(T key, out IPlatformAsset? value)
    {
        if (!_items.ContainsKey(key)) {
            value = null;
            return false;
        }

        value = _items[key];

        return true;
    }

    public bool Contains(T asset)
    {
        return _items.ContainsKey(asset);
    }
}
