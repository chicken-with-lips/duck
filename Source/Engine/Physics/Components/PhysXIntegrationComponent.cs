using Duck.ECS;
using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable, RuntimeComponent]
public partial struct PhysXIntegrationComponent
{
    public int BodyId;
}
