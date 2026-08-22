namespace Duck.Content;

public interface IAssetDatabase
{
    TAssetType? Register<TAssetType>(TAssetType asset, IAssetImportData? importData = null) where TAssetType : class, IAsset;
    IAsset? GetAsset(in AssetUri uri);
    TAssetType? GetAsset<TAssetType>(in AssetUri uri) where TAssetType : class, IAsset;
    TAssetType? GetAsset<TAssetType>(in AssetReference<TAssetType> assetReference) where TAssetType : class, IAsset;
}