using System.Collections.Concurrent;
using Duck.Logging;
using Silk.NET.Maths;

namespace Duck.Content;

public class ContentSubsystem : IContentSubsystem
{
    #region Properties

    #endregion

    #region Members

    private readonly AssetDatabase _database;
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<Guid, ITexture2D> _textureCache = new();
    private readonly ConcurrentDictionary<Guid, IMaterial> _materialCache = new();
    private readonly ConcurrentDictionary<Guid, IMaterialInstance> _materialInstanceCache = new();
    private readonly ConcurrentDictionary<Guid, IMesh> _meshCache = new();

    #endregion

    #region Methods

    public ContentSubsystem(ILogSubsystem logSubsystem)
    {
        _logger = logSubsystem.CreateLogger("Asset");
        _logger.LogInformation("Initializing asset subsystem.");

        _database = new AssetDatabase();
    }

    public AssetReference<T> GetReference<T>(Uri uri) where T : IAsset
    {
        return _database.GetReference<T>(uri);
    }

    public ITexture2D LoadTextureImmediate(AssetReference<ITexture2DAsset> assetReference)
    {
        throw new NotImplementedException();
        // var asset = _database.GetAsset(assetReference);
        //
        // if (_textureCache.ContainsKey(asset.Id)) {
        //     return _textureCache[asset.Id];
        // }
        //
        // var stream = File.OpenRead("./" + asset.Uri.AbsolutePath);
        // var imageResult = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        // var pixelBuffer = new PixelBufferDescriptor(imageResult.Data, PixelDataFormat.Rgba, PixelDataType.UByte);
        //
        // var internalTexture = TextureBuilder.Create()
        //     .WithWidth(imageResult.Width)
        //     .WithHeight(imageResult.Height)
        //     .WithLevels(1)
        //     .WithSampler(TextureSamplerType.Texture2d)
        //     .WithFormat(TextureFormat.Rgba8)
        //     .Build(_engine);
        //
        // internalTexture.SetImage(_engine, 0, pixelBuffer);
        // internalTexture.GenerateMipmaps(_engine);
        //
        // var texture = new Texture2D(asset, internalTexture);
        //
        // _textureCache.TryAdd(asset.Id, texture);
        //
        // return texture;
    }

    public IMaterial LoadMaterialImmediate(AssetReference<IMaterialAsset> assetReference)
    {
        throw new NotImplementedException();
        // var asset = _database.GetAsset(assetReference);
        //
        // if (_materialCache.ContainsKey(asset.Id)) {
        //     return _materialCache[asset.Id];
        // }
        //
        // var internalMaterial = MaterialBuilder.Create()
        //     .WithPackage(File.ReadAllBytes("./" + asset.Uri.AbsolutePath))
        //     .Build(_engine);
        //
        // var material = new Material(asset, internalMaterial);
        //
        // _materialCache.TryAdd(asset.Id, material);
        //
        // return material;
    }

    public IMaterialInstance LoadMaterialInstanceImmediate(AssetReference<IMaterialInstanceAsset> assetReference, Vector2D<float> scale)
    {
        throw new NotImplementedException();
        // var asset = _database.GetAsset(assetReference);
        //
        // // if (_materialInstanceCache.ContainsKey(asset.Id)) {
        // // return _materialInstanceCache[asset.Id];
        // // }
        //
        // var intermediate = LoadMaterialImmediate(asset.MaterialAssetReference);
        //
        // if (intermediate is not Material material) {
        //     throw new Exception("FIXME: not supported");
        // }
        //
        // var internalMaterial = material.InternalMaterial;
        // var internalMaterialInstance = internalMaterial.CreateInstance();
        // internalMaterialInstance.SetParameter("scale", scale.X, scale.Y);
        //
        // foreach (KeyValuePair<string, MaterialParameterCollection.IParameter> parameter in asset.Parameters) {
        //     if (parameter.Value is MaterialParameterCollection.Texture2DParameter textureParameter) {
        //         var texture = (Texture2D)LoadTextureImmediate(textureParameter.Value);
        //         internalMaterialInstance.SetParameter(textureParameter.Name, texture.InternalTexture, texture.CreateSampler());
        //     } else if (parameter.Value is MaterialParameterCollection.RgbaColor rgbaParameter) {
        //         internalMaterialInstance.SetParameter(rgbaParameter.Name, rgbaParameter.Type, rgbaParameter.Value);
        //     } else if (parameter.Value is MaterialParameterCollection.RgbColor rgbParameter) {
        //         internalMaterialInstance.SetParameter(rgbParameter.Name, rgbParameter.Type, rgbParameter.Value);
        //     } else {
        //         throw new Exception("FIXME: unhandled parameter type");
        //     }
        // }
        //
        // var materialInstance = new MaterialInstance(asset, internalMaterialInstance);
        //
        // // _materialInstanceCache.TryAdd(asset.Id, materialInstance);
        //
        // return materialInstance;
    }

    public IMesh LoadMeshImmediate(AssetReference<IMeshAsset> meshAssetReference, AssetReference<IMaterialInstanceAsset> materialInstanceAsset, Vector2D<float> scale)
    {
        throw new NotImplementedException();
        // // FIXME: properly load meshes, the MeshReader helper produces renderables which means we cant cache them
        // var asset = _database.GetAsset(meshAssetReference);
        //
        // // if (_meshCache.ContainsKey(asset.Id)) {
        // // return _meshCache[asset.Id];
        // // }
        //
        // var materialInstance = (MaterialInstance)LoadMaterialInstanceImmediate(materialInstanceAsset, scale);
        //
        // var internalMesh = MeshReader.LoadFromBuffer(
        //     _engine,
        //     File.ReadAllBytes("./" + asset.Uri.AbsolutePath),
        //     materialInstance.InternalMaterialInstance
        // );
        //
        // var mesh = new Mesh(asset, internalMesh);
        //
        // // _meshCache.TryAdd(asset.Id, mesh);
        //
        // return mesh;
    }

    #endregion
}
