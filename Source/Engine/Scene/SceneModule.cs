using System.Collections.Concurrent;
using Arch.Core;
using Arch.Core.Extensions;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Scene.Components;
using Duck.Scene.Events;
using Duck.Serialization;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Scene;

[AutoSerializable]
public partial class SceneModule : ISceneModule, IRenderableModule, IPreTickableModule, ITickableModule, IPostTickableModule, IHotReloadAwareModule
{
    #region Members

    private readonly IEventBus _eventBus;
    private readonly IGraphicsModule _graphicsModule;
    private readonly ConcurrentDictionary<string, IScene> _loadedScenes = new();

    private readonly ConcurrentBag<IScene> _pendingUnload = new();

    #endregion

    #region Methods

    public SceneModule(IEventBus eventBus, IGraphicsModule graphicsModule)
    {
        _eventBus = eventBus;
        _graphicsModule = graphicsModule;
    }

    public IScene Create(string name)
    {
        return Create(name, World.Create());
    }

    public IScene Create(string name, World world)
    {
        var scene = new Scene(name, world, _eventBus);

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        _eventBus.Emit(new SceneWasCreated() {
            Scene = scene
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
            _pendingUnload.TryTake(out var unused2);

            World.Destroy(scene.World);
        }

        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.PreTick(Time.DeltaFrame);
            }
        }
    }

    public void Tick()
    {
        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.Tick(Time.DeltaFrame);
            }
        }
    }

    public void PostTick()
    {
        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.PostTick(Time.DeltaFrame);
            }
        }
    }

    public void Render()
    {
        foreach (var kvp in _loadedScenes) {
            var scene = kvp.Value;
            var entities = new List<Entity>();

            scene.World.GetEntities(queryDescription: new QueryDescription().WithAll<RuntimeStaticMeshComponent>(), entities);

            foreach (var entity in entities) {
                var instanceComponent = entity.Get<RuntimeStaticMeshComponent>();
                var transformComponent = entity.Get<TransformComponent>();

                var renderObjectInstance = _graphicsModule.GraphicsDevice.GetRenderObjectInstance(instanceComponent.InstanceId);
                renderObjectInstance
                    .SetParameter("WorldScale", transformComponent.Scale)
                    .SetParameter("WorldPosition",
                        Matrix4X4.CreateScale(transformComponent.Scale)
                        * Matrix4X4.CreateFromQuaternion(transformComponent.Rotation)
                        * Matrix4X4.CreateTranslation(transformComponent.Position)
                    );

                if (entity.Has<BoundingBoxComponent>()) {
                    var boundingBox = entity.Get<BoundingBoxComponent>();

                    var scaledBoundingBox = new BoundingBoxComponent() {
                        Box = boundingBox.Box.GetScaled(transformComponent.Scale, boundingBox.Box.Center)
                    };

                    renderObjectInstance.BoundingVolume = boundingBox;
                } else if (entity.Has<BoundingSphereComponent>()) {
                    var boundingSphere = entity.Get<BoundingSphereComponent>();

                    // var scaledBoundingSphere = new BoundingSphereComponent() {
                    //     Radius = boundingSphere.Radius * MathF.Max(MathF.Max(transformComponent.Scale.X, transformComponent.Scale.Y), transformComponent.Scale.Z)
                    // };
                    var scaledBoundingSphere = new BoundingSphereComponent() {
                        Radius = boundingSphere.Radius
                    };

                    renderObjectInstance.BoundingVolume = boundingSphere;
                }

                _graphicsModule.GraphicsDevice.ScheduleRenderableInstance(instanceComponent.InstanceId);
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
