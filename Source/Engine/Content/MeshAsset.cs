namespace Duck.Content;

public struct MeshAsset : IMeshAsset
{
    public Guid Id { get; }
    public Uri Uri { get; }
    public AssetStatus Status { get; }

    public MeshAsset(Uri uri)
    {
        Id = Guid.NewGuid();
        Uri = uri;
        Status = AssetStatus.Unloaded;
    }
}

public struct Mesh : IMesh
{
    public IMeshAsset Asset { get; }

    public Mesh(IMeshAsset asset)
    {
        Asset = asset;
    }
}
