namespace Duck.Content;

public interface ISourceAssetImporter
{
    public Type OutputType { get; }
    public bool CanImport(string file);
}

public interface ISourceAssetImporter<TAsset> : ISourceAssetImporter
    where TAsset : class, IAsset
{
    public TAsset? Import(string file);
}
