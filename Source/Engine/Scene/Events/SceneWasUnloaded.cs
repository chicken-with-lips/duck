using Duck.ServiceBus;

namespace Duck.Scene.Events;

public struct SceneWasUnloaded : IEvent
{
    public IScene Scene;
}
