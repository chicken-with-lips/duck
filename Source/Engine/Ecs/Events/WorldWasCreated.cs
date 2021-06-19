using Duck.ServiceBus;

namespace Duck.Ecs.Events;

public struct WorldWasCreated : IEvent
{
    public IWorld World;
}
