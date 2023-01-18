using Duck.Ecs;
using Duck.ServiceBus;

namespace Duck.Physics.Events;

public struct PhysicsCollision : IEvent
{
    public IEntity A;
    public IEntity B;
}
