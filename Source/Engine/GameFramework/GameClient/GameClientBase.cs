using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.GameHost;
using Duck.Input;
using Duck.Scene;

namespace Duck.GameFramework.GameClient;

public abstract class GameClientBase : IGameClient
{
    #region Properties

    public IApplication Application => _application;

    #endregion

    #region Members

    private IScene? _scene;
    private ISystemComposition? _systemComposition;
    private IApplication? _application;

    #endregion

    #region Methods

    public T GetModule<T>()
        where T : IModule
    {
        return Application.GetModule<T>();
    }

    public virtual void Initialize(IGameClientInitializationContext context)
    {
        var app = context.Application;
        var sceneModule = app.GetModule<ISceneModule>();

        _application = app;
        _scene = sceneModule.Create();
        _systemComposition = CreateDefaultSystemComposition(_scene);

        InitializeGame(_systemComposition, context);

        _systemComposition.Init();
    }

    protected virtual ISystemComposition CreateDefaultSystemComposition(IScene scene)
    {
        if (_application is ApplicationBase app) {
            return app.CreateDefaultSystemComposition(scene);
        }

        return new SystemComposition(scene.World);
    }

    protected abstract void InitializeGame(ISystemComposition composition, IGameClientInitializationContext context);

    public virtual void Tick()
    {
        _systemComposition?.Tick();
    }

    #endregion
}
