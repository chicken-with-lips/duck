using Duck.ServiceBus;

namespace Duck.Scene.Events;

public readonly struct SceneWasLoaded : IEvent
{
    public readonly IScene Scene;

    public SceneWasLoaded(IScene scene)
    {
        Scene = scene;
    }
}
