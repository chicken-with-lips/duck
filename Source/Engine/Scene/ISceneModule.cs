namespace Duck.Scene;

public interface ISceneModule : IModule
{
    public IScene Create(string name);
    public void Unload(IScene scene);
}
