using Duck.Contracts.ServiceBus;

namespace Duck.Ecs.Events
{
    public struct WorldWasCreated : IEvent
    {
        public IWorld World;
    }
}
