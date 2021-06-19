using Duck.Contracts.AssetManagement;

namespace Duck.Rendering.Components
{
    public struct MeshComponent
    {
        public AssetReference<IMeshAsset> Mesh;
        public AssetReference<IMaterialInstanceAsset>[] Materials;
    }
}
