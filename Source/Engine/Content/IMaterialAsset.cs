namespace Duck.Content;

public interface IMaterialAsset : IAsset
{
}

public interface IMaterial
{
    public IMaterialAsset Asset { get; }
}
