using System.Collections.Concurrent;
using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.LowLevel.Jagged;
using Duck.Platform.ModuleManagement;
using Duck.Platform;
using Duck.Platform.Logging;
using Duck.Serialization;
using Duck.Utilities;

namespace Duck.Scene;

public class SceneModule : IInitializableModule, IDisposable, ITickModule, IFixedTickModule,
    IPreTickModule,
    IPostTickModule, IPresentSingleThreadedModule, IHotReloadSubscriberModule
{
    private readonly ConcurrentDictionary<string, Scene> _loadedScenes = new();

    private readonly Logger _logger;
    private HotReloadState? _hotReloadState;

    public SceneModule(Logger logger)
    {
        _logger = logger;
    }

    public void Dispose()
    {
        World.SharedJobScheduler?.Dispose();
    }

    public void FixedTick(FrameTimer frameTimer)
    {
        _loadedScenes
            .Where(s => s.Value.IsActive)
            .ForEach(kvp => kvp.Value.FixedTick(frameTimer));
    }

    public void BeginHotReload(HotReloadInstigator[] instigators)
    {
        _hotReloadState = new HotReloadState();
        _hotReloadState.Save(
            instigators,
            _loadedScenes
                .ToList()
                .ConvertAll(i => i.Value)
        );
    }

    public void EndHotReload()
    {
        Debug.Assert(_hotReloadState != null, "Hot reload state should not be null");

        _hotReloadState.Restore();
        _hotReloadState = null;
    }

    public void Initialize(IApplication app, IInitializationContext context)
    {
        if (context.WasHotReloaded) {
            return;
        }

        _logger.LogInformation("Initializing graphics module.");

        World.SharedJobScheduler = app.Scheduler;
    }

    public void PostTick(FrameTimer frameTimer)
    {
        _loadedScenes
            .Where(s => s.Value.IsActive)
            .ForEach(kvp => kvp.Value.PostTick(frameTimer));
    }

    public void Present(FrameTimer frameTimer)
    {
        _loadedScenes
            .Where(s => s.Value.IsActive)
            .ForEach(kvp => kvp.Value.Present(frameTimer));
    }

    public void PreTick(FrameTimer frameTimer)
    {
        _loadedScenes
            .Where(s => s.Value.IsActive)
            .ForEach(kvp => kvp.Value.PreTick(frameTimer));
    }

    public void Tick(FrameTimer frameTimer)
    {
        _loadedScenes
            .Where(s => s.Value.IsActive)
            .ForEach(kvp => kvp.Value.Tick(frameTimer));
    }

    public event Action<Scene>? SceneCreated;

    public Scene CreateScene(string name)
    {
        var scene = new Scene(name, World.Create());

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        SceneCreated?.Invoke(scene);

        scene.SystemRoot.Initialize();

        return scene;
    }

    public Scene GetOrCreateScene(string name)
    {
        var scene = FindScene(name);

        return scene ?? CreateScene(name);
    }

    public Scene? FindScene(string name)
    {
        return _loadedScenes.GetValueOrDefault(name);
    }

    private class HotReloadState
    {
        private readonly Dictionary<World, SerializedContainer> _state = new();

        public void Save(IList<HotReloadInstigator> instigators, IList<Scene> scenes)
        {
            List<Type> typesToRemove = [];
            List<Type> typesToReplace = [];

            Save_FindOrReplaceComponents(instigators, typesToRemove, typesToReplace);

            scenes
                .ForEach(s =>
                    {
                        Save_RemoveSystems(s, instigators);
                    }
                );

            scenes
                .Where(s => Save_IsRelevantWorld(s.World, typesToRemove, typesToReplace))
                .ForEach(s =>
                    {
                        Save_RemoveComponentFromEntities(s.World, typesToRemove);
                        Save_RemoveArchetypes(s.World, typesToRemove);
                        Save_SerializeWorld(s.World);
                        Save_PrepareWorldForReload(s.World);
                    }
                );

            Save_RemoveComponentsFromRegistry(typesToRemove);
        }

        public void Restore()
        {
            var context = new SerializationContext(true);

            _state.ForEach(s =>
                {
                    var container = s.Value;
                    var graphReader = new GraphReader(container.Data, container.Index, context);
                    var serializationFactory = Serializer.GetFactory<World>();

                    if (serializationFactory is WorldSerializationFactory worldSerializationFactory) {
                        worldSerializationFactory.World = s.Key;
                        worldSerializationFactory.DeserializeWorld(graphReader);
                    } else {
                        throw new Exception("Unexpected serialization factory");
                    }
                }
            );
        }

        private void Save_RemoveSystems(Scene scene, IList<HotReloadInstigator> instigators)
        {
            instigators.ForEach(i => scene.SystemRoot.RemoveByAssembly(i.Current));
        }

        private void Save_PrepareWorldForReload(World world)
        {
            world.ClearArchetypes();
            world.SetEntityDataArray(new JaggedArray<EntityData>(1));
            world.SetRecycledEntityIds([]);
            world.ClearQueryCache();
        }

        private void Save_SerializeWorld(World world)
        {
            var context = new SerializationContext(true);
            var graphWriter = new GraphWriter(context);
            var serializationFactory = Serializer.GetFactory<World>();
            serializationFactory.Serialize(world, graphWriter);

            _state.Add(world, graphWriter.Close());
        }

        private void Save_RemoveArchetypes(World world, IList<Type> types)
        {
            List<Archetype> toRemove = [];

            types.ForEach(t =>
                {
                    ComponentRegistry.TryGet(t, out var componentType);

                    foreach (var archetype in world.Archetypes.Items) {
                        if (archetype.Has(componentType) && !toRemove.Contains(archetype)) {
                            toRemove.Add(archetype);
                        }
                    }
                }
            );

            toRemove.ForEach(a => world.Archetypes.Remove(a));
        }

        private void Save_RemoveComponentFromEntities(World world, IList<Type> types)
        {
            types.ForEach(t =>
                {
                    ComponentRegistry.TryGet(t, out var componentType);

                    var query = new QueryDescription(new Signature(componentType));

                    // FIXME: optimize bulk removal
                    world.Query(
                        in query,
                        entity =>
                        {
                            world.Remove(entity, componentType);
                        }
                    );
                }
            );
        }

        private void Save_FindOrReplaceComponents(
            IList<HotReloadInstigator> instigators,
            IList<Type> typesToRemove,
            IList<Type> typesToReplace)
        {
            var instigatorCurrentAssemblies = instigators
                .ToList()
                .ConvertAll(i => i.Current);
            var instigatorNextAssemblies = instigators
                .ToList()
                .ConvertAll(i => i.Next);

            foreach (var oldType in ComponentRegistry.Types) {
                if (null == oldType) {
                    continue;
                }

                // We only care about types in assemblies we're about to reload.
                if (TypeUtil.GetType(oldType.FullName!, instigatorCurrentAssemblies) == null) {
                    continue;
                }

                // Does the type exist in the new assembly? 
                var typeFromNextAssemblies = TypeUtil.GetType(oldType.FullName!, instigatorNextAssemblies);

                if (typeFromNextAssemblies == null) {
                    // Remove the type entirely.
                    typesToRemove.Add(oldType);
                } else {
                    // Replace with the new type. 
                    ComponentRegistry.Replace(oldType, typeFromNextAssemblies);
                    typesToReplace.Add(typeFromNextAssemblies);
                }
            }
        }

        private bool Save_IsRelevantWorld(World world, IList<Type> typesToRemove, IList<Type> typesToReplace)
        {
            var combined = new List<Type>(typesToRemove);
            combined.AddRange(typesToReplace);

            foreach (var type in combined) {
                ComponentRegistry.TryGet(type, out var componentType);

                foreach (var archetype in world.Archetypes.Items) {
                    if (archetype.Has(componentType)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Save_RemoveComponentsFromRegistry(IList<Type> types)
        {
            types.ForEach(t => ComponentRegistry.Remove(t));
        }
    }
}