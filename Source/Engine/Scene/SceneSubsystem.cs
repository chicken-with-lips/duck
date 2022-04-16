using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneSubsystem : ISceneSubsystem, IHotReloadAwareSubsystem
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

    public void BeginHotReload()
    {
        _loadedScenes.Clear();
    }

    public void EndHotReload()
    {
    }

    #endregion
}
