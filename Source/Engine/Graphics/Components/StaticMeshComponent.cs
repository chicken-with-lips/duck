using Duck.Content;
using Duck.ECS.Components;
using Duck.Graphics.Mesh;
using Duck.Serialization;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct StaticMeshComponent
{
    public AssetReference<StaticMesh>? Mesh = default;
    // public AssetReference<MaterialInstance>[] Materials = new AssetReference<MaterialInstance>[] {};

    public StaticMeshComponent()
    {
    }
}

[AutoSerializable, RuntimeComponent]
public partial struct RuntimeStaticMeshComponent
{
    public uint InstanceId = default;

    public RuntimeStaticMeshComponent()
    {
    }
}
