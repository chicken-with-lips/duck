namespace Duck.Content;

public interface IAsset
{
    public Guid Id { get; }
    public AssetStatus Status { get; }
    public AssetImportData ImportData { get; }
    public bool IsLoaded { get; }

    public void ChangeStateTo(AssetStatus newStatus);
}

public interface IAsset<T> : IAsset
    where T : class, IAsset
{
    public AssetReference<T> MakeSharedReference();
    public AssetReference<T> MakeUniqueReference();
}

public enum AssetStatus
{
    Loading,
    Loaded,
    Reloading,
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
