using ChickenWithLips.PhysX.Net;
using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct PhysXIntegrationComponent
{
    public PxRigidActor Body;
}
