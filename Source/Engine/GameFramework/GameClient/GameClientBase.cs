using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.GameHost;
using Duck.GameFramework;
using Duck.Scene;

namespace Duck.GameFramework.GameClient;

public abstract class GameClientBase : IGameClient
{
    #region Members

    private IScene? _scene;
    private ISystemComposition? _systemComposition;
    private IApplication? _application;

    #endregion

    #region IClient

    public void Initialize(IGameClientInitializationContext context)
    {
        var app = context.Application;
        var sceneSubsystem = app.GetSubsystem<ISceneSubsystem>();

        _application = app;
        _scene = sceneSubsystem.Create();
        _systemComposition = CreateDefaultSystemComposition(_scene);

        // InitializeInput(app.GetSubsystem<IInputSubsystem>());
        InitializeSystems(_systemComposition, context);

        _systemComposition.Init();
    }

    // protected abstract void InitializeInput(IInputSubsystem input);

    protected virtual ISystemComposition CreateDefaultSystemComposition(IScene scene)
    {
        if (_application is ApplicationBase app) {
            return app.CreateDefaultSystemComposition(scene);
        }

        return new SystemComposition(scene.World);
    }

    protected abstract void InitializeSystems(ISystemComposition composition, IGameClientInitializationContext context);

    public void Tick()
    {
        _systemComposition?.Tick();
    }

    #endregion
}
