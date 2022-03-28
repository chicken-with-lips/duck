using Duck.Content;

namespace Duck.Graphics.Components
{
    public struct MeshComponent
    {
        public AssetReference<IMeshAsset> Mesh;
        public AssetReference<IMaterialInstanceAsset>[] Materials;
    }
}
