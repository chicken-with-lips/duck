using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.GameHost;
using Duck.GameFramework;
using Duck.Input;
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
        var sceneModule = app.GetModule<ISceneModule>();

        _application = app;
        _scene = sceneModule.Create();
        _systemComposition = CreateDefaultSystemComposition(_scene);

        InitializeInput(app.GetModule<IInputModule>());
        InitializeSystems(_systemComposition, context);

        _systemComposition.Init();
    }

    protected virtual ISystemComposition CreateDefaultSystemComposition(IScene scene)
    {
        if (_application is ApplicationBase app) {
            return app.CreateDefaultSystemComposition(scene);
        }

        return new SystemComposition(scene.World);
    }

    protected abstract void InitializeInput(IInputModule input);
    protected abstract void InitializeSystems(ISystemComposition composition, IGameClientInitializationContext context);

    public void Tick()
    {
        _systemComposition?.Tick();
    }

    #endregion
}
