using Duck.Ecs;

namespace Duck.Scene;

public class SceneSubsystem : ISceneSubsystem
{
    #region Members

    private readonly IWorldSubsystem _worldSubsystem;
    private readonly List<IScene> _loadedScenes = new();

    #endregion

    #region Methods

    public SceneSubsystem(IWorldSubsystem worldSubsystem)
    {
        _worldSubsystem = worldSubsystem;
    }

    public IScene Create()
    {
        var scene = new Scene(_worldSubsystem.Create());

        _loadedScenes.Add(scene);

        return scene;
    }

    #endregion
}
