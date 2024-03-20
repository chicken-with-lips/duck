using ADyn;
using ADyn.Shapes;
using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct RigidBodyBuilder<TShapeType> where TShapeType : unmanaged, IShape
{
    public RigidBodyDefinition<TShapeType> Definition;

    public RigidBodyBuilder()
    {
    }
}
