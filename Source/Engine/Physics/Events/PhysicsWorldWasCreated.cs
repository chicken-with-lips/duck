using Duck.ServiceBus;

namespace Duck.Physics.Events;

public readonly struct PhysicsWorldWasCreated : IEvent
{
    public readonly IPhysicsWorld World;

    public PhysicsWorldWasCreated(IPhysicsWorld world)
    {
        World = world;
    }
}
