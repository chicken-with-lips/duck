using System.Collections.Concurrent;

namespace Duck.Content;

public class AssetDatabase : IAssetDatabase
{
    private readonly ConcurrentDictionary<Guid, IAsset> _assetsByGuid = new();
    private readonly ConcurrentDictionary<Uri, IAsset> assetByUri = new();

    public AssetDatabase()
    {
    }

    public T Register<T>(T asset) where T : class, IAsset
    {
        if (assetByUri.ContainsKey(asset.ImportData.Uri)) {
            throw new Exception("FIXME: asset already registered: " + asset.ImportData.Uri);
        }

        if (_assetsByGuid.ContainsKey(asset.Id)) {
            throw new Exception("FIXME: asset already registered: " + asset.ImportData.Uri);
        }

        _assetsByGuid.AddOrUpdate(asset.Id, asset, (g, s) => asset);
        assetByUri.AddOrUpdate(asset.ImportData.Uri, asset, (s, g) => asset);

        return asset;
    }

    public IAsset? GetAsset(Uri uri)
    {
        if (!assetByUri.ContainsKey(uri)) {
            return null;
        }

        if (!assetByUri.TryGetValue(uri, out var asset)) {
            return null;
        }

        return asset;
    }

    public T? GetAsset<T>(Uri uri) where T : class, IAsset
    {
        return GetAsset(uri) as T;
    }

    public T GetAsset<T>(IAssetReference<T> assetReference) where T : class, IAsset
    {
        if (!_assetsByGuid.TryGetValue(assetReference.AssetId, out var asset)) {
            throw new Exception("FIXME: unknown asset");
        }

        return (T)asset;
    }
}
