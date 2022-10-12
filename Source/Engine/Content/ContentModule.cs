using System.Collections.Concurrent;
using System.Reflection;
using Duck.Logging;

namespace Duck.Content;

public class ContentModule : IContentModule
{
    #region Properties

    public IAssetDatabase Database => _database;
    public string ContentRootDirectory { get; set; }

    #endregion

    #region Members

    private readonly AssetDatabase _database;
    private readonly ILogger _logger;

    private readonly HashSet<IAssetLoader> _assetLoaders = new();
    private readonly HashSet<ISourceAssetImporter> _sourceAssetImporters = new();
    private readonly Dictionary<Type, IPlatformAssetCollection> _assetCaches = new();

    #endregion

    #region Methods

    public ContentModule(ILogModule logModule)
    {
        _logger = logModule.CreateLogger("Content");
        _logger.LogInformation("Created content module.");

        _database = new AssetDatabase();

        ContentRootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    }

    public IContentModule RegisterSourceAssetImporter(ISourceAssetImporter importer)
    {
        _sourceAssetImporters.Add(importer);

        return this;
    }

    public ISourceAssetImporter<TAsset>? FindSourceAssetImporter<TAsset>(string file)
        where TAsset : class, IAsset
    {
        file = Path.Combine(ContentRootDirectory, file);

        foreach (var sourceAssetImporter in _sourceAssetImporters) {
            if (sourceAssetImporter.OutputType == typeof(TAsset)
                && sourceAssetImporter.CanImport(file)) {
                return sourceAssetImporter as ISourceAssetImporter<TAsset>;
            }
        }

        return null;
    }

    public TAsset? Import<TAsset>(string file) where TAsset : class, IAsset
    {
        var importer = FindSourceAssetImporter<TAsset>(file);
        file = Path.Combine(ContentRootDirectory, file);

        if (null != importer) {
            _logger.LogInformation($"Importing \"{file}\"");
            
            var asset = importer.Import(file);

            if (null != asset) {
                Database.Register(asset);

                return asset;
            }
        } else {
            _logger.LogWarning($"\"{file}\" is not supported by any source asset importers");
        }

        return null;
    }

    public IContentModule RegisterAssetLoader<T, U>(IAssetLoader loader)
        where T : class, IAsset
        where U : class, IPlatformAsset
    {
        _assetLoaders.Add(loader);
        _assetCaches.Add(typeof(T), new PlatformAssetCollection<T, U>());

        return this;
    }

    public IAssetLoader? FindAssetLoader<T>(T asset, IAssetLoadContext context)
        where T : class, IAsset
    {
        foreach (var assetLoader in _assetLoaders) {
            if (assetLoader.CanLoad(asset, context)) {
                return assetLoader;
            }
        }

        return null;
    }

    private IPlatformAssetCollection<T> GetAssetCacheOrThrow<T>()
        where T : class, IAsset
    {
        if (_assetCaches.TryGetValue(typeof(T), out var cache)) {
            return cache as IPlatformAssetCollection<T>;
        }

        throw new Exception("FIXME: bad asset type");
    }

    public bool IsLoaded<T>(IAssetReference<T> assetReference)
        where T : class, IAsset
    {
        T? asset = _database.GetAsset<T>(assetReference);

        return asset is { IsLoaded: true };
    }

    public IPlatformAsset LoadImmediate<T>(IAssetReference<T> assetReference, IAssetLoadContext context, byte[]? fixmeData = null)
        where T : class, IAsset
    {
        // FIXME: dont pass data directly like this

        var cache = GetAssetCacheOrThrow<T>();

        if (!cache.Contains(assetReference)) {
            T? asset = _database.GetAsset<T>(assetReference);

            if (asset == null) {
                throw new Exception("FIXME: unknown asset");
            }

            _logger.LogInformation("Loading {0}", asset.ImportData.Uri);

            IAssetLoader? assetLoader = FindAssetLoader(asset, context);

            if (assetLoader == null) {
                throw new Exception("FIXME: unknown loader");
            }

            ReadOnlySpan<byte> data;

            // fixme: replace with more appropriate asset location and loading
            if (asset.ImportData.Uri.IsFile) {
                var tmp = asset.ImportData.Uri.AbsolutePath;
                var path = tmp.StartsWith(ContentRootDirectory) ? tmp : ContentRootDirectory + tmp;
                data = File.ReadAllBytes(path);
            } else {
                data = fixmeData;
            }

            var platformAsset = assetLoader.Load(asset, context, data);
            asset.ChangeStateToLoaded();

            cache.Add(assetReference, platformAsset);
        }

        return cache.GetBase(assetReference);
    }

    public IPlatformAsset LoadImmediate<T>(IAssetReference<T> assetReference) where T : class, IAsset
    {
        return LoadImmediate(assetReference, EmptyAssetLoadContext.Default);
    }

    // public ITexture2D LoadTextureImmediate(AssetReference<ITexture2DAsset> assetReference)
    // {
    //     throw new NotImplementedException();
    //     // var asset = _database.GetAsset(assetReference);
    //     //
    //     // if (_textureCache.ContainsKey(asset.Id)) {
    //     //     return _textureCache[asset.Id];
    //     // }
    //     //
    //     // var stream = File.OpenRead("./" + asset.Uri.AbsolutePath);
    //     // var imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
    //     // var pixelBuffer = new PixelBufferDescriptor(imageResult.Data, PixelDataFormat.Rgba, PixelDataType.UByte);
    //     //
    //     // var internalTexture = TextureBuilder.Create()
    //     //     .WithWidth(imageResult.Width)
    //     //     .WithHeight(imageResult.Height)
    //     //     .WithLevels(1)
    //     //     .WithSampler(TextureSamplerType.Texture2d)
    //     //     .WithFormat(TextureFormat.Rgba8)
    //     //     .Build(_engine);
    //     //
    //     // internalTexture.SetImage(_engine, 0, pixelBuffer);
    //     // internalTexture.GenerateMipmaps(_engine);
    //     //
    //     // var texture = new Texture2D(asset, internalTexture);
    //     //
    //     // _textureCache.TryAdd(asset.Id, texture);
    //     //
    //     // return texture;
    // }
    //
    // public IMaterial LoadMaterialImmediate(AssetReference<IMaterialAsset> assetReference)
    // {
    //     throw new NotImplementedException();
    //     // var asset = _database.GetAsset(assetReference);
    //     //
    //     // if (_materialCache.ContainsKey(asset.Id)) {
    //     //     return _materialCache[asset.Id];
    //     // }
    //     //
    //     // var internalMaterial = MaterialBuilder.Create()
    //     //     .WithPackage(File.ReadAllBytes("./" + asset.Uri.AbsolutePath))
    //     //     .Build(_engine);
    //     //
    //     // var material = new Material(asset, internalMaterial);
    //     //
    //     // _materialCache.TryAdd(asset.Id, material);
    //     //
    //     // return material;
    // }
    //
    // public IMaterialInstance LoadMaterialInstanceImmediate(AssetReference<IMaterialInstanceAsset> assetReference, Vector2D<float> scale)
    // {
    //     throw new NotImplementedException();
    //     // var asset = _database.GetAsset(assetReference);
    //     //
    //     // // if (_materialInstanceCache.ContainsKey(asset.Id)) {
    //     // // return _materialInstanceCache[asset.Id];
    //     // // }
    //     //
    //     // var intermediate = LoadMaterialImmediate(asset.MaterialAssetReference);
    //     //
    //     // if (intermediate is not Material material) {
    //     //     throw new Exception("FIXME: not supported");
    //     // }
    //     //
    //     // var internalMaterial = material.InternalMaterial;
    //     // var internalMaterialInstance = internalMaterial.CreateInstance();
    //     // internalMaterialInstance.SetParameter("scale", scale.X, scale.Y);
    //     //
    //     // foreach (KeyValuePair<string, MaterialParameterCollection.IParameter> parameter in asset.Parameters) {
    //     //     if (parameter.Value is MaterialParameterCollection.Texture2DParameter textureParameter) {
    //     //         var texture = (Texture2D)LoadTextureImmediate(textureParameter.Value);
    //     //         internalMaterialInstance.SetParameter(textureParameter.Name, texture.InternalTexture, texture.CreateSampler());
    //     //     } else if (parameter.Value is MaterialParameterCollection.RgbaColor rgbaParameter) {
    //     //         internalMaterialInstance.SetParameter(rgbaParameter.Name, rgbaParameter.Type, rgbaParameter.Value);
    //     //     } else if (parameter.Value is MaterialParameterCollection.RgbColor rgbParameter) {
    //     //         internalMaterialInstance.SetParameter(rgbParameter.Name, rgbParameter.Type, rgbParameter.Value);
    //     //     } else {
    //     //         throw new Exception("FIXME: unhandled parameter type");
    //     //     }
    //     // }
    //     //
    //     // var materialInstance = new MaterialInstance(asset, internalMaterialInstance);
    //     //
    //     // // _materialInstanceCache.TryAdd(asset.Id, materialInstance);
    //     //
    //     // return materialInstance;
    // }
    //
    // public IMesh LoadMeshImmediate(AssetReference<IMeshAsset> meshAssetReference, AssetReference<IMaterialInstanceAsset> materialInstanceAsset, Vector2D<float> scale)
    // {
    //     throw new NotImplementedException();
    //     // // FIXME: properly load meshes, the MeshReader helper produces renderables which means we cant cache them
    //     // var asset = _database.GetAsset(meshAssetReference);
    //     //
    //     // // if (_meshCache.ContainsKey(asset.Id)) {
    //     // // return _meshCache[asset.Id];
    //     // // }
    //     //
    //     // var materialInstance = (MaterialInstance)LoadMaterialInstanceImmediate(materialInstanceAsset, scale);
    //     //
    //     // var internalMesh = MeshReader.LoadFromBuffer(
    //     //     _engine,
    //     //     File.ReadAllBytes("./" + asset.Uri.AbsolutePath),
    //     //     materialInstance.InternalMaterialInstance
    //     // );
    //     //
    //     // var mesh = new Mesh(asset, internalMesh);
    //     //
    //     // // _meshCache.TryAdd(asset.Id, mesh);
    //     //
    //     // return mesh;
    // }

    #endregion
}
