using Duck.Ecs;

namespace Duck.Scene;

public interface ISceneModule : IModule
{
    public IScene Create(string name);
    public IScene Create(string name, IWorld world);
    public void Unload(IScene scene);

    public IScene GetOrCreateScene(string name);
    public IScene[] GetLoadedScenes();
    public IScene? GetLoadedScene(string name);
}
