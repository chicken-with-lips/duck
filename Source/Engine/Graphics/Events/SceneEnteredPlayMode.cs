using Duck.ServiceBus;

namespace Duck.Graphics.Events;

public struct SceneEnteredPlayMode : IEvent
{
    public IScene Scene;
}
