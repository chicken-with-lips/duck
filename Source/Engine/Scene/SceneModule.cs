using System.Collections.Concurrent;
using Duck.Ecs;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Scene.Events;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneModule : ISceneModule, IPreRenderableModule, IPreTickableModule, ITickableModule, IHotReloadAwareModule
{
    #region Members

    private readonly IEcsModule _ecsModule;
    private readonly IGraphicsModule _graphicsModule;
    private readonly ConcurrentDictionary<string, IScene> _loadedScenes = new();

    private readonly ConcurrentBag<IScene> _pendingUnload = new();

    #endregion

    #region Methods

    public SceneModule(IEcsModule ecsModule, IGraphicsModule graphicsModule)
    {
        _ecsModule = ecsModule;
        _graphicsModule = graphicsModule;
    }

    public IScene Create(string name)
    {
        return Create(name, _ecsModule.Create());
    }

    public IScene Create(string name, IWorld world)
    {
        var scene = new Scene(name, world);

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        Console.WriteLine("SceneWasCreated: " + scene.Name);

        world.CreateOneShot((ref SceneWasCreated cmp) => {
            cmp.Scene = scene;
        });

        return scene;
    }

    public IScene GetOrCreateScene(string name)
    {
        var scene = GetLoadedScene(name);

        if (null != scene) {
            return scene;
        }

        return Create(name);
    }

    public void Unload(IScene scene)
    {
        if (!_pendingUnload.Contains(scene)) {
            _pendingUnload.Add(scene);
        }
    }

    public IScene[] GetLoadedScenes()
    {
        return _loadedScenes.Values.ToArray();
    }

    public IScene? GetLoadedScene(string name)
    {
        if (_loadedScenes.TryGetValue(name, out var scene)) {
            return scene;
        }

        return null;
    }

    public void PreTick()
    {
        foreach (var scene in _pendingUnload) {
            if (!_loadedScenes.ContainsKey(scene.Name)) {
                continue;
            }

            _loadedScenes.Remove(scene.Name, out var unused);
            _ecsModule.Destroy(scene.World);
        }
    }

    public void Tick()
    {
        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.Tick();
            }
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
