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

    public UniqueAssetReference<T> MakeUniqueReference()
    {
        return new UniqueAssetReference<T>(Id);
    }

    public SharedAssetReference<T> MakeSharedReference()
    {
        return new SharedAssetReference<T>(Id);
    }

    #endregion
}
