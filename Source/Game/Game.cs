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

    protected override void RegisterSubsystems()
    {
        base.RegisterSubsystems();

        AddSubsystem(new GameClientHostSubsystem(this, GetSubsystem<ILogSubsystem>()));
    }
}
