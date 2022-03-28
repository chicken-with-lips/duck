using Duck.ServiceBus;

namespace Duck.Physics.Events;

public struct PhysicsWorldWasCreated : IEvent
{
    public IPhysicsWorld World;
}
