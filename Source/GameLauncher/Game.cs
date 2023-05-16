using Duck.GameFramework;
using Duck.Logging;
using Duck.Platform;
using Duck.Renderer;
using Game.Host;

namespace Game;

public class Game : ApplicationBase
{
    public Game(IPlatform platform, IRenderSystem renderSystem, bool isEditor)
        : base(platform, renderSystem, isEditor)
    {
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new GameClientHostModule(this, GetModule<ILogModule>()));
    }
}
