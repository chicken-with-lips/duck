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

    public IApplication Application {
        get {
            if (_application == null) {
                throw new Exception("TODO: errors");
            }

            return _application;
        }
    }

    #endregion

    #region Members

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

        _application = app;

        InitializeClient(context);
    }


    protected abstract void InitializeClient(IGameClientInitializationContext context);

    public virtual void Tick()
    {
    }

    #endregion
}
