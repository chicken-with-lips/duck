namespace Duck.Scene;

public interface ISceneSubsystem : IApplicationSubsystem
{
    public IScene Create();
}
