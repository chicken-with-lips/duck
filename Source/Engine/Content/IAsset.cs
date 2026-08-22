namespace Duck.Content;

public interface IAsset
{
    Guid Id { get; }
    AssetUri Uri { get; }
    AssetState State { get; }
    bool IsLoaded { get; }

    void ChangeStateTo(AssetState newState);
}

public interface IAsset<T> : IAsset
    where T : class, IAsset
{
    AssetReference<T> MakeSharedReference();
    AssetReference<T> MakeUniqueReference();
}

public enum AssetState
{
    Loading,
    Loaded,
    Reloading,
    Unloaded,
}

public interface IAssetImportData;

public struct LocalFileImportData : IAssetImportData
{
    public string OriginalPath { get; }

    public LocalFileImportData(string originalPath)
    {
        OriginalPath = originalPath;
    }
}