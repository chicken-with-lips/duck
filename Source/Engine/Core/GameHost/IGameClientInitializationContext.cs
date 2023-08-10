namespace Duck.GameHost;

public interface IGameClientInitializationContext
{
    public IApplication Application { get; }
    public bool IsHotReload { get; }
}
