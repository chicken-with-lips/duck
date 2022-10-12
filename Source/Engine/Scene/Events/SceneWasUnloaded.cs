using Duck.ServiceBus;

namespace Duck.Scene.Events;

public readonly struct SceneWasUnloaded : IEvent
{
    public readonly IScene Scene;

    public SceneWasUnloaded(IScene scene)
    {
        Scene = scene;
    }
}
