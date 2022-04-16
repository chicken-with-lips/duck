using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneModule : ISceneModule, IHotReloadAwareModule
{
    #region Members

    private readonly IWorldModule _worldModule;
    private readonly List<IScene> _loadedScenes = new();

    #endregion

    #region Methods

    public SceneModule(IWorldModule worldModule)
    {
        _worldModule = worldModule;
    }

    public IScene Create()
    {
        var scene = new Scene(_worldModule.Create());

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
