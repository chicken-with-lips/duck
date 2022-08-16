using System.Collections.Concurrent;

namespace Duck.Content;

public class AssetDatabase : IAssetDatabase
{
    private readonly ConcurrentDictionary<Guid, IAsset> _assetsByGuid = new();
    // private readonly ConcurrentDictionary<string, Guid> _guidByUri = new();

    public AssetDatabase()
    {
    }

    public T Register<T>(T asset) where T : class, IAsset
    {
        // if (_guidByUri.ContainsKey(uri.ToString())) {
        // throw new Exception("FIXME: asset already registered");
        // }

        if (_assetsByGuid.ContainsKey(asset.Id)) {
            throw new Exception("FIXME: asset already registered");
        }

        _assetsByGuid.AddOrUpdate(asset.Id, asset, (g, s) => asset);
        // _guidByUri.AddOrUpdate(asset.Uri.ToString(), asset.Id, (s, g) => asset.Id);

        return asset;
    }

    // public AssetReference<T> GetReference<T>(Uri uri) where T : IAsset
    // {
    //     if (!_guidByUri.ContainsKey(uri.ToString())) {
    //         throw new Exception("FIXME: asset doesn't exist: " + uri.ToString());
    //     }
    //
    //     _guidByUri.TryGetValue(uri.ToString(), out var id);
    //
    //     return new AssetReference<T>(id);
    // }

    public T GetAsset<T>(IAssetReference<T> assetReference) where T : class, IAsset
    {
        if (!_assetsByGuid.TryGetValue(assetReference.AssetId, out var asset)) {
            throw new Exception("FIXME: unknown asset");
        }

        return (T)asset;
    }
}
