using Duck.ServiceBus;

namespace Duck.Renderer.Events;

public struct SceneEnteredPlayMode : IEvent
{
    public IScene Scene;
}
