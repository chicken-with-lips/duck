using Duck.ServiceBus;

namespace Duck.Scene.Events;

public readonly struct SceneWasMadeActive : IEvent
{
    public readonly IScene Scene;

    public SceneWasMadeActive(IScene scene)
    {
        Scene = scene;
    }
}
