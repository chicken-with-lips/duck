using Duck.Platform;
using Duck.Graphics;

namespace Duck.GameFramework;

public class InstancedApplication : ApplicationBase
{
    private readonly IApplication _app;


    public InstancedApplication(IApplication app, IPlatform platform, IRenderSystem renderSystem, bool isEditor)
        : base(platform, renderSystem, isEditor)
    {
        _app = app;
    }

    public override void Shutdown()
    {
        // no-op
    }

    public override T GetModule<T>()
    {
        foreach (var proxy in Modules) {
            if (proxy is T cast) {
                return cast;
            }
        }

        return _app.GetModule<T>();
    }

    protected override void RegisterModules()
    {
        foreach (var module in _app.Modules) {
            if (module is IModuleCanBeInstanced proxyable) {
                AddModule(proxyable.CreateModuleInstance(this));
            }
        }
    }
}
