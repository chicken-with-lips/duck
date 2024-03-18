using Duck.Platform;

namespace Duck.GameHost;

public interface IGameClientInitializationContext
{
    public IApplication Application { get; }
    public bool IsHotReload { get; }
}
