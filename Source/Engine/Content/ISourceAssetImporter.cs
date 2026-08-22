namespace Duck.Content;

public interface IAssetImporter
{
    Type OutputType { get; }
    bool CanImport(Uri source);
}

public interface IAssetImporter<out TAsset> : IAssetImporter
    where TAsset : class, IAsset
{
    TAsset? Import(Uri source);
}