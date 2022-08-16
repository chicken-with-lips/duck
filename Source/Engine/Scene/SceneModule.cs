using System.Collections.Concurrent;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Scene.Events;
using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneModule : ISceneModule, IPreRenderableModule, ITickableModule, IHotReloadAwareModule
{
    #region Members

    private readonly IEcsModule _ecsModule;
    private readonly IGraphicsModule _graphicsModule;
    private readonly IEventBus _eventBus;
    private readonly ConcurrentDictionary<string, IScene> _loadedScenes = new();

    #endregion

    #region Methods

    public SceneModule(IEcsModule ecsModule, IGraphicsModule graphicsModule, IEventBus eventBus)
    {
        _ecsModule = ecsModule;
        _graphicsModule = graphicsModule;
        _eventBus = eventBus;
    }

    public IScene Create(string name)
    {
        var scene = new Scene(name, _ecsModule.Create());

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        _eventBus.Enqueue(new SceneWasLoaded(scene));

        return scene;
    }

    public void Tick()
    {
        foreach (var kvp in _loadedScenes) {
            kvp.Value.SystemComposition.Tick();
        }
    }

    public void PreRender()
    {
        foreach (var kvp in _loadedScenes) {
            var scene = kvp.Value;

            foreach (var entityId in scene.Renderables) {
                if (scene.World.HasComponent<RenderableInstanceComponent>(entityId)) {
                    var instanceComponent = scene.World.GetComponent<RenderableInstanceComponent>(entityId);
                    _graphicsModule.GraphicsDevice.ScheduleRenderableInstance(instanceComponent.Id);
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
