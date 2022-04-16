namespace Duck.Scene;

public interface ISceneModule : IModule
{
    public IScene Create();
}
