using Duck.Contracts.ServiceBus;

namespace Duck.Game.Events
{
    public struct WorldWasCreated : IEvent
    {
        public IWorld World;
    }
}
