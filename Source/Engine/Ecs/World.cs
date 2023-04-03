using System.Collections.Concurrent;
using Duck.Ecs.Systems;
using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.Ecs;

[AutoSerializable]
public partial class World : IWorld
{
    private const int OneShotEntityFrameLifetime = 1;

    #region Properties

    public ISystemComposition SystemComposition => _systemComposition;

    public bool IsActive {
        get => _isActive;
        set => _isActive = value;
    }

    #endregion

    #region Members

    private bool _isActive;
    private EntityPool _entityPool;
    private ComponentPoolCollection _componentPools;

    private readonly FilterEvaluator _filterEvaluator = new();
    private readonly Dictionary<string, IFilter> _filters = new();
    private readonly ISystemComposition _systemComposition;

    private readonly ConcurrentBag<int> _entitiesAdded1 = new();
    private readonly ConcurrentBag<int> _entitiesAdded2 = new();

    private readonly ConcurrentBag<int> _entitiesRemoved1 = new();
    private readonly ConcurrentBag<int> _entitiesRemoved2 = new();

    private ConcurrentBag<int> _entitiesAddedPreviousFrame;
    private ConcurrentBag<int> _entitiesAddedCurrentFrame;

    private ConcurrentBag<int> _entitiesRemovedPreviousFrame;
    private ConcurrentBag<int> _entitiesRemovedCurrentFrame;

    private readonly ConcurrentDictionary<int, int> _oneShotEntities = new();

    #endregion

    public World()
        : this(WorldConfiguration.Default)
    {
    }

    ~World()
    {
        Dispose(false);
    }

    public World(WorldConfiguration config)
    {
        _isActive = true;
        _systemComposition = new SystemComposition(this);
        _entityPool = new EntityPool(this, config.EntityPoolInitialSize);
        _componentPools = new ComponentPoolCollection(config.ComponentPoolCount, config.ComponentPoolInitialSize);

        _entitiesAddedCurrentFrame = _entitiesAdded1;
        _entitiesAddedPreviousFrame = _entitiesAdded2;

        _entitiesRemovedCurrentFrame = _entitiesRemoved1;
        _entitiesRemovedPreviousFrame = _entitiesRemoved2;
    }

    #region IWorld

    public IFilter[] Filters => _filters.Values.ToArray();

    private void InitFilters()
    {
        ThrowIfDisposed();

        // FIXME:
        // this runs to initialize the filters with any components that are preloaded in to the system (e.g. loading
        // a save. the way this is structured filters will be initialized twice for any system that creates
        // components. we should instead detect which components were added as part of preloading and only process
        // those.

        for (var entityId = 0; entityId < _entityPool.Count; entityId++) {
            EvaluateFilters(_entityPool.Get(entityId), true, true);
        }
    }

    private void InitFilter(IFilter filter)
    {
        ThrowIfDisposed();

        for (var entityId = 0; entityId < _entityPool.Count; entityId++) {
            EvaluateFilter(filter, _entityPool.Get(entityId), true);
        }
    }

    public void BeginFrame()
    {
        Console.WriteLine("BEGIN FRAME");
        Console.WriteLine("------------------------");
        ThrowIfDisposed();

        SwapCreatedEntityBuffers();
        FlushPendingEntities();
        RemoveComponentsFromDeletedEntities();

        foreach (var filter in Filters) {
            filter.SwapDirtyBuffers();
        }

        SwapDeletedEntityBuffers();
    }

    public void EndFrame()
    {
        Console.WriteLine("END FRAME");
        Console.WriteLine("------------------------");
        
        ThrowIfDisposed();

        RemoveDeletedEntities();
    }

    public void Tick()
    {
        if (_isActive) {
            SystemComposition.Tick();
        }
    }

    public IEntity CreateEntity()
    {
        ThrowIfDisposed();

        var entity = _entityPool.Allocate();
        _entitiesAddedCurrentFrame.Add(entity.Id);

        return entity;
    }

    public IEntity CreateOneShot<TEvent>(ComponentInitializer<TEvent> initializer) where TEvent : struct
    {
        ThrowIfDisposed();

        var entity = CreateEntity();
        entity.IsOneShot = true;
        
        Console.WriteLine("DEBUG: CreateOneShot: " + entity.Id);

        ref var data = ref entity.Get<TEvent>();

        initializer(ref data);

        return entity;
    }

    public void DeleteEntity(IEntity entity)
    {
        ThrowIfDisposed();

        DeleteEntity(entity.Id);
    }

    public void DeleteEntity(int entityId)
    {
        ThrowIfDisposed();

        if (!_entitiesAddedCurrentFrame.Contains(entityId)) {
            _entitiesRemovedCurrentFrame.Add(entityId);
        }
    }

    private void FlushPendingEntities()
    {
        foreach (var entityId in _entitiesAddedPreviousFrame) {
            var entity = GetEntity(entityId);

            if (!entity.IsPending) {
                continue;
            }

            entity.IsPending = false;

            if (entity.IsOneShot) {
                if (!_oneShotEntities.TryAdd(entity.Id, OneShotEntityFrameLifetime)) {
                    Console.WriteLine("DEBUG: AddOneShot: " + entity.Id);
                    _oneShotEntities[entity.Id] = OneShotEntityFrameLifetime;
                } else {
                    Console.WriteLine("DEBUG: AddOneShot reset: " + entity.Id);
                }
            }

            EvaluateFilters(entity, true, true);
        }
    }

    public ComponentReference AllocateComponent<T>(IEntity entity) where T : struct
    {
        ThrowIfDisposed();

        return _componentPools.AllocateComponent<T>(entity);
    }

    public void DeallocateComponent(Type componentType, int componentIndex)
    {
        ThrowIfDisposed();

        _componentPools.DeallocateComponent(componentType, componentIndex);
    }

    public void DeallocateComponent<T>(int componentIndex) where T : struct
    {
        ThrowIfDisposed();

        DeallocateComponent(typeof(T), componentIndex);
    }

    public void InternalNotifyComponentAllocated(IEntity entity, ComponentReference componentReference)
    {
        ThrowIfDisposed();

        if (!entity.IsPending) {
            EvaluateFilters(_entityPool.Get(componentReference.EntityId), true, _oneShotEntities.ContainsKey(entity.Id));
        }
    }

    public void InternalNotifyComponentDeallocated(IEntity entity)
    {
        ThrowIfDisposed();

        if (!entity.IsPending) {
            EvaluateFilters(entity, false, _oneShotEntities.ContainsKey(entity.Id));
        }
    }

    public Type GetTypeFromIndex(int typeIndex)
    {
        ThrowIfDisposed();

        return _componentPools.GetTypeFromIndex(typeIndex);
    }

    public int GetTypeIndexForComponent<T>() where T : struct
    {
        ThrowIfDisposed();

        return _componentPools.GetTypeIndexForComponent<T>();
    }

    public int GetTypeIndexForComponent(Type type)
    {
        ThrowIfDisposed();

        return _componentPools.GetTypeIndexForComponent(type);
    }

    public ComponentReference GetComponentReference<T>(int typeIndex, int componentIndex) where T : struct
    {
        ThrowIfDisposed();

        return _componentPools.GetComponentReference<T>(typeIndex, componentIndex);
    }

    public IEntity GetEntity(int entityId)
    {
        ThrowIfDisposed();

        return _entityPool.Get(entityId);
    }

    public IEntity[] GetEntitiesByComponent<T>() where T : struct
    {
        ThrowIfDisposed();

        List<IEntity> ents = new();

        for (var entityId = 0; entityId < _entityPool.Count; entityId++) {
            if (HasComponent<T>(entityId)) {
                ents.Add(GetEntity(entityId));
            }
        }

        return ents.ToArray();
    }

    public bool IsEntityAllocated(int entityId)
    {
        ThrowIfDisposed();

        return _entityPool.IsAllocated(entityId);
    }

    public ref T GetComponent<T>(int typeIndex, int componentIndex) where T : struct
    {
        ThrowIfDisposed();

        return ref _componentPools.GetComponent<T>(typeIndex, componentIndex);
    }

    public ref T GetComponent<T>(ComponentReference componentReference) where T : struct
    {
        ThrowIfDisposed();

        return ref GetComponent<T>(componentReference.TypeIndex, componentReference.ComponentIndex);
    }

    public ref T GetComponent<T>(int entityId) where T : struct
    {
        ThrowIfDisposed();

        return ref GetEntity(entityId).Get<T>();
    }

    public bool HasComponent<T>(int entityId) where T : struct
    {
        ThrowIfDisposed();

        return IsEntityAllocated(entityId) && GetEntity(entityId).Has<T>();
    }

    public IFilter<T> CompileFilter<T>(IFilter<T> filter) where T : struct
    {
        ThrowIfDisposed();

        if (!_filters.ContainsKey(filter.Id)) {
            _filters[filter.Id] = filter;
        }

        InitFilter(_filters[filter.Id]);

        return (Filter<T>)_filters[filter.Id];
    }

    public IFilter<T1, T2> CompileFilter<T1, T2>(IFilter<T1, T2> filter)
        where T1 : struct
        where T2 : struct
    {
        ThrowIfDisposed();

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
        ThrowIfDisposed();

        if (!_filters.ContainsKey(filter.Id)) {
            _filters[filter.Id] = filter;
        }

        return (Filter<T1, T2, T3>)_filters[filter.Id];
    }

    #endregion

    private void EvaluateFilters(IEntity entity, bool isAddition, bool isOneShot)
    {
        ThrowIfDisposed();

        foreach (var filter in _filters.Values) {
            var evalResult = _filterEvaluator.Evaluate(filter, entity);

            if (evalResult && isAddition) {
                filter.AddEntity(entity, isOneShot);
            } else if (!evalResult && !isAddition) {
                filter.RemoveEntity(entity, isOneShot);
            }
        }
    }

    private void EvaluateFilter(IFilter filter, IEntity entity, bool isOneShot)
    {
        ThrowIfDisposed();

        var evalResult = _filterEvaluator.Evaluate(filter, entity);

        if (evalResult) {
            filter.AddEntity(entity, isOneShot);
        }
    }

    private void RemoveComponentsFromDeletedEntities()
    {
        ThrowIfDisposed();

        foreach (var entityId in _entitiesRemovedCurrentFrame) {
            GetEntity(entityId).RemoveAll();
        }

        foreach (var kvp in _oneShotEntities) {
            if (kvp.Value <= 0) {
                Console.WriteLine("DEBUG: RemoveOneShot: " + kvp.Key);
                GetEntity(kvp.Key).RemoveAll();
            }
        }
    }

    private void RemoveDeletedEntities()
    {
        ThrowIfDisposed();

        foreach (var entityId in _entitiesRemovedPreviousFrame) {
            if (!_oneShotEntities.ContainsKey(entityId)) {
                _entityPool.Deallocate(
                    GetEntity(entityId)
                );
            }
        }

        foreach (var kvp in _oneShotEntities) {
            if (kvp.Value <= 0) {
                Console.WriteLine("DEBUG: Dealloc oneshot: " + kvp.Key);
                
                _entityPool.Deallocate(
                    GetEntity(kvp.Key)
                );

                _oneShotEntities.TryRemove(kvp.Key, out var unused);
            } else {
                Console.WriteLine("DEBUG: decr OneShot: " + kvp.Key);
                _oneShotEntities[kvp.Key]--;
            }
        }
    }

    private void SwapCreatedEntityBuffers()
    {
        _entitiesAddedPreviousFrame = _entitiesAddedCurrentFrame;
        _entitiesAddedCurrentFrame = _entitiesAddedCurrentFrame == _entitiesAdded1 ? _entitiesAdded2 : _entitiesAdded1;
        _entitiesAddedCurrentFrame.Clear();
    }

    private void SwapDeletedEntityBuffers()
    {
        _entitiesRemovedPreviousFrame = _entitiesRemovedCurrentFrame;
        _entitiesRemovedCurrentFrame = _entitiesRemovedCurrentFrame == _entitiesRemoved1 ? _entitiesRemoved2 : _entitiesRemoved1;
        _entitiesRemovedCurrentFrame.Clear();
    }

    #region IDisposable

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        IsDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (IsDisposed) {
            throw new ObjectDisposedException("World");
        }
    }

    #endregion
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

public delegate void ComponentInitializer<TEvent>(ref TEvent cmp) where TEvent : struct;
