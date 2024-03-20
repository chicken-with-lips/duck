using Duck.ServiceBus;

namespace Duck.Physics.Events;

public readonly struct PhysicsWorldWasCreated : IEvent
{
    public readonly IPhysicsScene Scene;

    public PhysicsWorldWasCreated(IPhysicsScene scene)
    {
        Scene = scene;
    }
}
