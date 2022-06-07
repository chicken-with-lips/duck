using Duck.Ecs;
using Duck.ServiceBus;

namespace Duck.Physics.Events;

public readonly struct PhysicsCollision : IEvent
{
    public readonly IEntity A;
    public readonly IEntity B;

    public PhysicsCollision(IEntity a, IEntity b)
    {
        A = a;
        B = b;
    }
}
