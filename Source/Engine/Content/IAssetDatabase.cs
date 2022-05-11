namespace Duck.Content;

public interface IAssetDatabase
{
    T Register<T>(T asset) where T : class, IAsset;
    // AssetReference<T> GetReference<T>(Uri uri) where T : IAsset;
    T GetAsset<T>(AssetReference<T> assetReference) where T : class, IAsset;
}
