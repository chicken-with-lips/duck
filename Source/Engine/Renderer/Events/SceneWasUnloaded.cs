using Duck.ServiceBus;

namespace Duck.Renderer.Events;

public struct SceneWasUnloaded : IEvent
{
    public IScene Scene;
}
