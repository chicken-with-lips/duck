using Silk.NET.Maths;

namespace Duck.Content;

public interface IContentModule : IModule
{
    public AssetReference<T> GetReference<T>(Uri uri) where T : IAsset;
    public ITexture2D LoadTextureImmediate(AssetReference<ITexture2DAsset> assetReference);
    public IMaterial LoadMaterialImmediate(AssetReference<IMaterialAsset> assetReference);
    public IMaterialInstance LoadMaterialInstanceImmediate(AssetReference<IMaterialInstanceAsset> assetReference, Vector2D<float> scale);
    public IMesh LoadMeshImmediate(AssetReference<IMeshAsset> meshAssetReference, AssetReference<IMaterialInstanceAsset> materialInstanceAsset, Vector2D<float> scale);
}

public readonly struct AssetReference<T> where T : IAsset
{
    public readonly Guid Id;

    public AssetReference(Guid id)
    {
        Id = id;
    }
}
