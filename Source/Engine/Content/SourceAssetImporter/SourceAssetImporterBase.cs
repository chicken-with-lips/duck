namespace Duck.Content.SourceAssetImporter;

public abstract class SourceAssetImporterBase<TAsset> : ISourceAssetImporter<TAsset>
    where TAsset : class, IAsset
{
    public Type OutputType => typeof(TAsset);

    public abstract bool CanImport(string file);
    public abstract TAsset Import(string file);
}
