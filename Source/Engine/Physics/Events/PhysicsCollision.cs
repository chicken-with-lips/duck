using Arch.Core;
using Duck.ServiceBus;

namespace Duck.Physics.Events;

public struct PhysicsCollision : IEvent
{
    public EntityReference A;
    public EntityReference B;
}
