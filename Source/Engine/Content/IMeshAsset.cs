namespace Duck.Content;

public interface IMeshAsset : IAsset
{
}

public interface IMesh
{
    public IMeshAsset Asset { get; }
}
