using Duck.ServiceBus;

namespace Duck.Scene.Events;

public struct SceneWasCreated : IEvent
{
    public IScene Scene;
}
