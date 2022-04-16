using Duck.GameFramework;
using Duck.Logging;
using Game.Host;

namespace Game;

public class Game : ApplicationBase
{
    public Game(bool isEditor)
        : base(isEditor)
    {
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new GameClientHostModule(this, GetModule<ILogModule>()));
    }
}
