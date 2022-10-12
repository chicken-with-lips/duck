namespace Duck.Scene.Scripting;

public interface ISceneScript
{
}

public interface ISceneLoaded : ISceneScript
{
    public void OnLoaded();
}

public interface ISceneUnloaded : ISceneScript
{
    public void OnUnloaded();
}

public interface ISceneMadeActive : ISceneScript
{
    public void OnActivated();
}

public interface ISceneTick : ISceneScript
{
    public void OnTick();
}
