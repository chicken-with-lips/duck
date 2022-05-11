using Duck.Content;
using Duck.Ecs;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Scene.Components;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneModule : ISceneModule, IPreRenderableModule, IHotReloadAwareModule
{
    #region Members

    private readonly IEcsModule _ecsModule;
    private readonly IGraphicsModule _graphicsModule;
    private readonly IContentModule _contentModule;
    private readonly List<IScene> _loadedScenes = new();

    #endregion

    #region Methods

    public SceneModule(IEcsModule ecsModule, IGraphicsModule graphicsModule, IContentModule contentModule)
    {
        _ecsModule = ecsModule;
        _graphicsModule = graphicsModule;
        _contentModule = contentModule;
    }

    public IScene Create()
    {
        var scene = new Scene(_ecsModule.Create());

        _loadedScenes.Add(scene);

        return scene;
    }

    public void PreRender()
    {
        foreach (var scene in _loadedScenes) {
            foreach (var entityId in scene.Renderables) {
                var transformComponent = scene.World.GetComponent<TransformComponent>(entityId);

                if (scene.World.HasComponent<RenderableInstanceComponent>(entityId)) {
                    var instanceComponent = scene.World.GetComponent<RenderableInstanceComponent>(entityId);
                    _graphicsModule.GraphicsDevice.ScheduleRenderable(instanceComponent.Id);
                }
            }
        }
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
