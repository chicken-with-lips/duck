using Duck.ServiceBus;

namespace Duck.Scene.Events;

public struct SceneWasMadeActive : IEvent
{
    public IScene Scene;
}
