namespace Duck.Content;

public interface IAssetDatabase
{
    T Register<T>(T asset) where T : class, IAsset;
    IAsset? GetAsset(Uri uri);
    T GetAsset<T>(Uri uri) where T : class, IAsset;
    T GetAsset<T>(IAssetReference<T> assetReference) where T : class, IAsset;
}
