using Duck.Content;
using Duck.Graphics.Mesh;
using Duck.Serialization;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct MeshComponent
{
    public IAssetReference<StaticMesh>? Mesh = default;
    // public AssetReference<MaterialInstance>[] Materials = new AssetReference<MaterialInstance>[] {};
}
