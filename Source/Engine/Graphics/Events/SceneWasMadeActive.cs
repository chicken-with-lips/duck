using Duck.ServiceBus;

namespace Duck.Graphics.Events;

public struct SceneWasMadeActive : IEvent
{
    public IScene Scene;
}
