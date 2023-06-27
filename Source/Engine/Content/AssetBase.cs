namespace Duck.Content;

public abstract class AssetBase<T> : IAsset<T>
    where T : class, IAsset
{
    #region Properties

    public Guid Id { get; }
    public AssetStatus Status { get; private set; }
    public AssetImportData ImportData { get; }
    public bool IsLoaded => Status == AssetStatus.Loaded;

    #endregion

    #region Methods

    public AssetBase(AssetImportData importData)
    {
        Id = Guid.NewGuid();
        Status = AssetStatus.Unloaded;
        ImportData = importData;
    }

    public void ChangeStateTo(AssetStatus newStatus)
    {
        Status = newStatus;
    }

    public AssetReference<T> MakeUniqueReference()
    {
        return AssetReference<T>.Unique(Id);
    }

    public AssetReference<T> MakeSharedReference()
    {
        return AssetReference<T>.Shared(Id);
    }

    #endregion
}
