namespace Duck.Content;

public interface IImportPipeline
{
    ImportPipeline RegisterAssetImporter(IAssetImporter importer);
    IAssetImporter<TAsset>? FindAssetImporter<TAsset>(Uri source)
        where TAsset : class, IAsset;
    TAsset? Import<TAsset>(Uri source)
        where TAsset : class, IAsset;
}