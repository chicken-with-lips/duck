using ADyn;
using ADyn.Shapes;
using Duck.Serialization;

namespace Duck.Physics.Components;

[DuckSerializable]
public struct BoxRigidBodyBuilder
{
    public RigidBodyDefinition<BoxShape> Definition;

    public BoxRigidBodyBuilder()
    {
    }
}

[DuckSerializable]
public struct SphereRigidBodyBuilder
{
    public RigidBodyDefinition<SphereShape> Definition;

    public SphereRigidBodyBuilder()
    {
    }
}
