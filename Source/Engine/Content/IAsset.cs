namespace Duck.Content;

public interface IAsset
{
    public Guid Id { get; }
    public Uri Uri { get; }
    public AssetStatus Status { get; }
}

public enum AssetStatus
{
    Loaded,
    Unloaded,
}
