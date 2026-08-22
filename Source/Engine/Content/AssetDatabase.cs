using System.Collections.Concurrent;
using Duck.Platform.Logging;

namespace Duck.Content;

public class AssetDatabase : IAssetDatabase
{
    private readonly Logger _logger;
    private readonly ConcurrentDictionary<Guid, IAsset> _assetsById = new();
    private readonly ConcurrentDictionary<Guid, IAssetImportData> _importDataById = new();
    private readonly ConcurrentDictionary<AssetUri, IAsset> _assetByUri = new();

    public AssetDatabase(Logger logger)
    {
        _logger = logger;
    }

    public TAssetType? Register<TAssetType>(TAssetType asset, IAssetImportData? importData = null) where TAssetType : class, IAsset
    {
        if (_assetByUri.ContainsKey(asset.Uri)) {
            _logger.LogError(
                $"Asset with this address already registered: {asset.Uri}"
            );

            return null;
        }

        if (_assetsById.ContainsKey(asset.Id)) {
            _logger.LogError(
                $"Asset with this ID is already registered: {asset.Id}"
            );

            return null;
        }

        _assetsById.TryAdd(asset.Id, asset);
        _assetByUri.TryAdd(asset.Uri, asset);

        if (null != importData) {
            _importDataById.TryAdd(asset.Id, importData);
        }

        return asset;
    }

    public IAsset? GetAsset(in AssetUri uri)
    {
        if (!_assetByUri.TryGetValue(uri, out var asset)) {
            _logger.LogError(
                $"Asset not found: {uri}"
            );
        }

        return asset;
    }

    public TAssetType? GetAsset<TAssetType>(in AssetUri uri) where TAssetType : class, IAsset
    {
        var asset = GetAsset(uri);

        if (null == asset) {
            return null;
        }

        if (asset is not TAssetType outAsset) {
            _logger.LogError(
                $"Asset is not of the expected type: {typeof(TAssetType)}"
            );

            return null;
        }

        return outAsset;
    }

    public TAssetType? GetAsset<TAssetType>(in AssetReference<TAssetType> assetReference) where TAssetType : class, IAsset
    {
        if (!_assetsById.TryGetValue(assetReference.AssetId, out var asset)) {
            _logger.LogError(
                $"Asset not found: {assetReference.AssetId}"
            );

            return null;
        }

        if (asset is not TAssetType outAsset) {
            _logger.LogError(
                $"Asset is not of the expected type: {typeof(TAssetType)}"
            );
            return null;
        }

        return outAsset;
    }
}