using System.Collections.Concurrent;
using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial class World : IWorld
{
    #region Members

    private EntityPool _entityPool;
    private ComponentPoolCollection _componentPools;

    private readonly FilterEvaluator _filterEvaluator = new();
    private readonly Dictionary<string, IFilter> _filters = new();

    private readonly ConcurrentBag<int> _entitiesRemoved1 = new();
    private readonly ConcurrentBag<int> _entitiesRemoved2 = new();

    private ConcurrentBag<int> _entitiesRemovedPreviousFrame;
    private ConcurrentBag<int> _entitiesRemovedCurrentFrame;

    #endregion

    public World()
        : this(WorldConfiguration.Default)
    {
    }

    public World(WorldConfiguration config)
    {
        _entityPool = new EntityPool(this, config.EntityPoolInitialSize);
        _componentPools = new ComponentPoolCollection(config.ComponentPoolCount, config.ComponentPoolInitialSize);

        _entitiesRemovedCurrentFrame = _entitiesRemoved1;
        _entitiesRemovedPreviousFrame = _entitiesRemoved2;
    }

    #region IWorld

    public IFilter[] Filters => _filters.Values.ToArray();

    public void InitFilters()
    {
        // FIXME:
        // this runs to initialize the filters with any components that are preloaded in to the system (e.g. loading
        // a save. the way this is structured filters will be initialized twice for any system that creates
        // components. we should instead detect which components were added as part of preloading and only process
        // those.

        for (var entityId = 0; entityId < _entityPool.Count; entityId++) {
            EvaluateFilters(_entityPool.Get(entityId), true);
        }
    }

    public void BeginFrame()
    {
        RemoveComponentsFromDeletedEntities();

        foreach (var filter in Filters) {
            filter.SwapDirtyBuffers();
        }

        SwapDeletedEntityBuffers();
    }

    public void EndFrame()
    {
        RemoveDeletedEntities();
    }

    public IEntity CreateEntity()
    {
        return _entityPool.Allocate();
    }

    public void DeleteEntity(IEntity entity)
    {
        DeleteEntity(entity.Id);
    }

    public void DeleteEntity(int entityId)
    {
        _entitiesRemovedCurrentFrame.Add(entityId);
    }

    public ComponentReference AllocateComponent<T>(IEntity entity) where T : struct
    {
        return _componentPools.AllocateComponent<T>(entity);
    }

    public void DeallocateComponent(Type componentType, int componentIndex)
    {
        _componentPools.DeallocateComponent(componentType, componentIndex);
    }

    public void DeallocateComponent<T>(int componentIndex) where T : struct
    {
        DeallocateComponent(typeof(T), componentIndex);
    }

    public void InternalNotifyComponentAllocated(ComponentReference componentReference)
    {
        EvaluateFilters(_entityPool.Get(componentReference.EntityId), true);
    }

    public void InternalNotifyComponentDeallocated(IEntity entity)
    {
        EvaluateFilters(entity, false);
    }

    public Type GetTypeFromIndex(int typeIndex)
    {
        return _componentPools.GetTypeFromIndex(typeIndex);
    }

    public int GetTypeIndexForComponent<T>() where T : struct
    {
        return _componentPools.GetTypeIndexForComponent<T>();
    }

    public int GetTypeIndexForComponent(Type type)
    {
        return _componentPools.GetTypeIndexForComponent(type);
    }

    public ComponentReference GetComponentReference<T>(int typeIndex, int componentIndex) where T : struct
    {
        return _componentPools.GetComponentReference<T>(typeIndex, componentIndex);
    }

    public IEntity GetEntity(int entityId)
    {
        return _entityPool.Get(entityId);
    }

    public bool IsEntityAllocated(int entityId)
    {
        return _entityPool.IsAllocated(entityId);
    }

    public ref T GetComponent<T>(int typeIndex, int componentIndex) where T : struct
    {
        return ref _componentPools.GetComponent<T>(typeIndex, componentIndex);
    }

    public ref T GetComponent<T>(ComponentReference componentReference) where T : struct
    {
        return ref GetComponent<T>(componentReference.TypeIndex, componentReference.ComponentIndex);
    }

    public ref T GetComponent<T>(int entityId) where T : struct
    {
        return ref GetEntity(entityId).Get<T>();
    }

    public bool HasComponent<T>(int entityId) where T : struct
    {
        return GetEntity(entityId).Has<T>();
    }

    public IFilter<T> CompileFilter<T>(IFilter<T> filter) where T : struct
    {
        if (!_filters.ContainsKey(filter.Id)) {
            _filters[filter.Id] = filter;
        }

        return (Filter<T>)_filters[filter.Id];
    }

    public IFilter<T1, T2> CompileFilter<T1, T2>(IFilter<T1, T2> filter)
        where T1 : struct
        where T2 : struct
    {
        if (!_filters.ContainsKey(filter.Id)) {
            _filters[filter.Id] = filter;
        }

        return (Filter<T1, T2>)_filters[filter.Id];
    }

    public IFilter<T1, T2, T3> CompileFilter<T1, T2, T3>(IFilter<T1, T2, T3> filter)
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        if (!_filters.ContainsKey(filter.Id)) {
            _filters[filter.Id] = filter;
        }

        return (Filter<T1, T2, T3>)_filters[filter.Id];
    }

    #endregion

    private void EvaluateFilters(IEntity entity, bool isAddition)
    {
        foreach (var filter in _filters.Values) {
            var evalResult = _filterEvaluator.Evaluate(filter, entity);

            if (evalResult && isAddition && !filter.EntityList.Contains(entity.Id)) {
                filter.QueueAddition(entity);
            } else if (!evalResult && !isAddition && filter.EntityList.Contains(entity.Id)) {
                filter.QueueRemoval(entity);
            }
        }
    }

    private void RemoveComponentsFromDeletedEntities()
    {
        foreach (var entityId in _entitiesRemovedCurrentFrame) {
            GetEntity(entityId).RemoveAll();
        }
    }

    private void RemoveDeletedEntities()
    {
        foreach (var entityId in _entitiesRemovedPreviousFrame) {
            _entityPool.Deallocate(
                GetEntity(entityId)
            );
        }
    }

    private void SwapDeletedEntityBuffers()
    {
        _entitiesRemovedPreviousFrame = _entitiesRemovedCurrentFrame;
        _entitiesRemovedCurrentFrame = _entitiesRemovedCurrentFrame == _entitiesRemoved1 ? _entitiesRemoved2 : _entitiesRemoved1;
        _entitiesRemovedCurrentFrame.Clear();
    }
}

public class WorldConfiguration
{
    public readonly int EntityPoolInitialSize;
    public readonly int ComponentPoolCount;
    public readonly int ComponentPoolInitialSize;

    public WorldConfiguration(int entityPoolInitialSize, int componentPoolCount, int componentPoolInitialSize)
    {
        EntityPoolInitialSize = entityPoolInitialSize;
        ComponentPoolCount = componentPoolCount;
        ComponentPoolInitialSize = componentPoolInitialSize;
    }

    public static WorldConfiguration Default => new WorldConfiguration(
        1024,
        1024,
        1024
    );
}
