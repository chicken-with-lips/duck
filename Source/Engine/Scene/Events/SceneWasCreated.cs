using Duck.ServiceBus;

namespace Duck.Scene.Events;

public readonly struct SceneWasCreated : IEvent
{
    public readonly IScene Scene;

    public SceneWasCreated(IScene scene)
    {
        Scene = scene;
    }
}
