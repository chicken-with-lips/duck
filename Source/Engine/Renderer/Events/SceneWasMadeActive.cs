using Duck.ServiceBus;

namespace Duck.Renderer.Events;

public struct SceneWasMadeActive : IEvent
{
    public IScene Scene;
}
