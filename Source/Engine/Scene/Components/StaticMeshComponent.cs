using Duck.Content;
using Duck.Graphics.Mesh;
using Duck.Serialization;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct StaticMeshComponent
{
    public SharedAssetReference<StaticMesh>? Mesh = default;
    // public AssetReference<MaterialInstance>[] Materials = new AssetReference<MaterialInstance>[] {};
}

[AutoSerializable]
public partial struct RuntimeStaticMeshComponent
{
    public uint InstanceId = default;
}
