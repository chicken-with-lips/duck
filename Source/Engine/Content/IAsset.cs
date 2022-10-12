namespace Duck.Content;

public interface IAsset
{
    public Guid Id { get; }
    public AssetStatus Status { get; }
    public AssetImportData ImportData { get; }
    public bool IsLoaded { get; }

    public void ChangeStateToLoaded();
}

public interface IAsset<T> : IAsset
    where T : class, IAsset
{
    public SharedAssetReference<T> MakeSharedReference();
    public UniqueAssetReference<T> MakeUniqueReference();
}

public enum AssetStatus
{
    Loaded,
    Unloaded,
}

public struct AssetImportData
{
    public Uri Uri { get; }

    public AssetImportData(Uri uri)
    {
        Uri = uri;
    }
}
