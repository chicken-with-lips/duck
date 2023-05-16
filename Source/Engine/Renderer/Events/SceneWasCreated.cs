using Duck.ServiceBus;

namespace Duck.Renderer.Events;

public struct SceneWasCreated : IEvent
{
    public IScene Scene;
}
