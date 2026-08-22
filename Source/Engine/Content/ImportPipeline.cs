using Duck.Platform.Logging;

namespace Duck.Content;

public class ImportPipeline : IImportPipeline
{
    private readonly IContentModule _contentModule;
    private readonly Logger _logger;
    private readonly HashSet<IAssetImporter> _assetImporters = new();

    public ImportPipeline(IContentModule contentModule, Logger logger)
    {
        _contentModule = contentModule;
        _logger = logger;

    }

    public ImportPipeline RegisterAssetImporter(IAssetImporter importer)
    {
        lock (_assetImporters) {
            _assetImporters.Add(importer);
        }

        return this;
    }

    public IAssetImporter<TAsset>? FindAssetImporter<TAsset>(Uri source)
        where TAsset : class, IAsset
    {
        lock (_assetImporters) {
            foreach (var importer in _assetImporters) {
                if (importer.OutputType == typeof(TAsset)
                    && importer.CanImport(source)) {
                    return importer as IAssetImporter<TAsset>;
                }
            }
        }

        return null;
    }

    public TAsset? Import<TAsset>(Uri source)
        where TAsset : class, IAsset
    {
        var importer = FindAssetImporter<TAsset>(source);

        if (null != importer) {
            _logger.LogInformation($"Importing \"{source}\"");

            var asset = importer.Import(source);

            if (null != asset) {
                _contentModule.Database.Register(asset);

                return asset;
            }
        } else {
            _logger.LogWarning($"\"{source}\" is not supported by any asset importers");
        }

        return null;
    }
}