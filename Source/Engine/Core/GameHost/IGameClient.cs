namespace Duck.GameHost;

public interface IGameClient
{
    public void Initialize(IGameClientInitializationContext context);

    public void Tick();
}
