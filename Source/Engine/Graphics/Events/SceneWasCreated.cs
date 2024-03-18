using Duck.ServiceBus;

namespace Duck.Graphics.Events;

public struct SceneWasCreated : IEvent
{
    public IScene Scene;
}
