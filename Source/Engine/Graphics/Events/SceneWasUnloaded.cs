using Duck.ServiceBus;

namespace Duck.Graphics.Events;

public struct SceneWasUnloaded : IEvent
{
    public IScene Scene;
}
