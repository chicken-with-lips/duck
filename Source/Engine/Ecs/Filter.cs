using System.Collections.Concurrent;
using System.Text;

namespace Duck.Ecs;

public abstract class FilterBase : IFilter
{
    public IWorld World {
        get;

        // FIXME: used to restore the World reference after a reload. serialization library doesn't handle circular
        // references
        internal set;
    }

    public string Id { get; }

    public abstract int[] EntityList { get; }
    public abstract int[] EntityAddedList { get; }
    public abstract int[] EntityRemovedList { get; }

    public FilterBase(IWorld world, string id, FilterComponentPredicate[] componentPredicates)
    {
        World = world;
        Id = id;
        ComponentPredicates = componentPredicates;
    }

    #region IFilter

    public FilterComponentPredicate[] ComponentPredicates { get; }

    public abstract void QueueAddition(IEntity entity);
    public abstract void QueueRemoval(IEntity entity);

    public abstract void SwapDirtyBuffers();

    #endregion
}

public class Filter<T> : FilterBase, IFilter<T>
    where T : struct
{
    private readonly ConcurrentDictionary<int, ComponentReference> _entityMap = new();

    private readonly ConcurrentDictionary<int, ComponentReference> _entitiesAdded1 = new();
    private readonly ConcurrentDictionary<int, ComponentReference> _entitiesAdded2 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved1 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved2 = new();

    private ConcurrentDictionary<int, ComponentReference> _entitiesAddedPreviousFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedPreviousFrame;

    private ConcurrentDictionary<int, ComponentReference> _entitiesAddedCurrentFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedCurrentFrame;

    public Filter(IWorld world, string id, FilterComponentPredicate[] componentPredicates)
        : base(world, id, componentPredicates)
    {
        _entitiesAddedPreviousFrame = _entitiesAdded2;
        _entitiesRemovedPreviousFrame = _entitiesRemoved2;

        _entitiesAddedCurrentFrame = _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemoved1;
    }

    #region FilterBase

    public override int[] EntityList => _entityMap.Keys.ToArray();
    public override int[] EntityAddedList => _entitiesAddedPreviousFrame.Keys.ToArray();
    public override int[] EntityRemovedList => _entitiesRemovedPreviousFrame.Keys.ToArray();

    public override void QueueAddition(IEntity entity)
    {
        _entitiesAddedCurrentFrame.TryAdd(entity.Id, entity.GetComponentReference<T>());
    }

    public override void QueueRemoval(IEntity entity)
    {
        _entitiesRemovedCurrentFrame.Remove(entity.Id, out var unused);
    }

    public override void SwapDirtyBuffers()
    {
        foreach (var kvp in _entitiesAddedCurrentFrame) {
            _entityMap.TryAdd(kvp.Key, kvp.Value);
        }

        foreach (var kvp in _entitiesRemovedCurrentFrame) {
            _entityMap.TryRemove(kvp.Key, out var unused);
        }

        _entitiesAddedPreviousFrame = _entitiesAddedCurrentFrame;
        _entitiesRemovedPreviousFrame = _entitiesRemovedCurrentFrame;

        _entitiesAddedCurrentFrame = _entitiesAddedCurrentFrame == _entitiesAdded1 ? _entitiesAdded2 : _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemovedCurrentFrame == _entitiesRemoved1 ? _entitiesRemoved2 : _entitiesRemoved1;

        _entitiesAddedCurrentFrame.Clear();
        _entitiesRemovedCurrentFrame.Clear();
    }

    #endregion

    #region IFilter<T>

    public IEntity GetEntity(int entityId)
    {
        return World.GetEntity(entityId);
    }

    public ref T Get(int entityId)
    {
        return ref World.GetComponent<T>(_entityMap[entityId]);
    }

    public ref T GetAdded(int entityId)
    {
        return ref World.GetComponent<T>(_entitiesAddedPreviousFrame[entityId]);
    }

    #endregion
}

public class Filter<T1, T2> : FilterBase, IFilter<T1, T2>
    where T1 : struct
    where T2 : struct
{
    private Dictionary<int, Tuple<ComponentReference, ComponentReference>> _entityMap = new();

    private readonly ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference>> _entitiesAdded1 = new();
    private readonly ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference>> _entitiesAdded2 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved1 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved2 = new();

    private ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference>> _entitiesAddedPreviousFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedPreviousFrame;

    private ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference>> _entitiesAddedCurrentFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedCurrentFrame;

    public Filter(IWorld world, string id, FilterComponentPredicate[] componentPredicates)
        : base(world, id, componentPredicates)
    {
        _entitiesAddedCurrentFrame = _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemoved1;
    }

    public override void QueueAddition(IEntity entity)
    {
        _entitiesAddedCurrentFrame.TryAdd(entity.Id, new Tuple<ComponentReference, ComponentReference>(
            entity.GetComponentReference<T1>(),
            entity.GetComponentReference<T2>()
        ));
    }

    public override void QueueRemoval(IEntity entity)
    {
        _entitiesRemovedCurrentFrame.TryRemove(entity.Id, out var unused);
    }

    public IEntity GetEntity(int entityId)
    {
        return World.GetEntity(entityId);
    }

    public ref T1 Get1(int entityId)
    {
        return ref World.GetComponent<T1>(_entityMap[entityId].Item1);
    }

    public ref T2 Get2(int entityId)
    {
        return ref World.GetComponent<T2>(_entityMap[entityId].Item2);
    }

    #region FilterBase

    public override int[] EntityList => _entityMap.Keys.ToArray();
    public override int[] EntityAddedList => _entitiesAddedPreviousFrame.Keys.ToArray();
    public override int[] EntityRemovedList => _entitiesRemovedPreviousFrame.Keys.ToArray();

    public override void SwapDirtyBuffers()
    {
        foreach (var kvp in _entitiesAddedCurrentFrame) {
            _entityMap.TryAdd(kvp.Key, kvp.Value);
        }

        foreach (var kvp in _entitiesRemovedCurrentFrame) {
            _entityMap.Remove(kvp.Key);
        }

        _entitiesAddedPreviousFrame = _entitiesAddedCurrentFrame;
        _entitiesRemovedPreviousFrame = _entitiesRemovedCurrentFrame;

        _entitiesAddedCurrentFrame = _entitiesAddedCurrentFrame == _entitiesAdded1 ? _entitiesAdded2 : _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemovedCurrentFrame == _entitiesRemoved1 ? _entitiesRemoved2 : _entitiesRemoved1;

        _entitiesAddedCurrentFrame.Clear();
        _entitiesRemovedCurrentFrame.Clear();
    }

    #endregion
}

public class Filter<T1, T2, T3> : FilterBase, IFilter<T1, T2, T3>
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    private Dictionary<int, Tuple<ComponentReference, ComponentReference, ComponentReference>> _entityMap = new();

    private readonly ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference, ComponentReference>> _entitiesAdded1 = new();
    private readonly ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference, ComponentReference>> _entitiesAdded2 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved1 = new();
    private readonly ConcurrentDictionary<int, int> _entitiesRemoved2 = new();

    private ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference, ComponentReference>> _entitiesAddedPreviousFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedPreviousFrame;

    private ConcurrentDictionary<int, Tuple<ComponentReference, ComponentReference, ComponentReference>> _entitiesAddedCurrentFrame;
    private ConcurrentDictionary<int, int> _entitiesRemovedCurrentFrame;

    public Filter(IWorld world, string id, FilterComponentPredicate[] componentPredicates)
        : base(world, id, componentPredicates)
    {
        _entitiesAddedCurrentFrame = _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemoved1;
    }

    public override void QueueAddition(IEntity entity)
    {
        _entitiesAddedCurrentFrame.TryAdd(entity.Id, new Tuple<ComponentReference, ComponentReference, ComponentReference>(
            entity.GetComponentReference<T1>(),
            entity.GetComponentReference<T2>(),
            entity.GetComponentReference<T3>()
        ));
    }

    public override void QueueRemoval(IEntity entity)
    {
        _entitiesRemovedCurrentFrame.TryRemove(entity.Id, out var unused);
    }

    public IEntity GetEntity(int entityId)
    {
        return World.GetEntity(entityId);
    }

    public ref T1 Get1(int entityId)
    {
        return ref World.GetComponent<T1>(_entityMap[entityId].Item1);
    }

    public ref T2 Get2(int entityId)
    {
        return ref World.GetComponent<T2>(_entityMap[entityId].Item2);
    }

    public ref T3 Get3(int entityId)
    {
        return ref World.GetComponent<T3>(_entityMap[entityId].Item3);
    }

    #region FilterBase

    public override int[] EntityList => _entityMap.Keys.ToArray();
    public override int[] EntityAddedList => _entitiesAddedPreviousFrame.Keys.ToArray();
    public override int[] EntityRemovedList => _entitiesRemovedPreviousFrame.Keys.ToArray();

    public override void SwapDirtyBuffers()
    {
        foreach (var kvp in _entitiesAddedCurrentFrame) {
            _entityMap.TryAdd(kvp.Key, kvp.Value);
        }

        foreach (var kvp in _entitiesRemovedCurrentFrame) {
            _entityMap.Remove(kvp.Key);
        }

        _entitiesAddedPreviousFrame = _entitiesAddedCurrentFrame;
        _entitiesRemovedPreviousFrame = _entitiesRemovedCurrentFrame;

        _entitiesAddedCurrentFrame = _entitiesAddedCurrentFrame == _entitiesAdded1 ? _entitiesAdded2 : _entitiesAdded1;
        _entitiesRemovedCurrentFrame = _entitiesRemovedCurrentFrame == _entitiesRemoved1 ? _entitiesRemoved2 : _entitiesRemoved1;

        _entitiesAddedCurrentFrame.Clear();
        _entitiesRemovedCurrentFrame.Clear();
    }

    #endregion
}

public abstract class FilterBuilder
{
    public IWorld World => _world;
    public FilterIdGenerator IdGenerator => _idGenerator;
    public FilterComponentPredicate[] ComponentPredicates => _componentPredicates.ToArray();

    private readonly IWorld _world;
    private readonly FilterIdGenerator _idGenerator = new();
    private readonly List<FilterComponentPredicate> _componentPredicates = new();

    public FilterBuilder(IWorld world)
    {
        _world = world;
    }

    protected void AddWith(Type componentType)
    {
        _idGenerator.Add("with:" + componentType.FullName);
        _componentPredicates.Add(entity => entity.Has(componentType));
    }

    protected void AddWithout(Type componentType)
    {
        _idGenerator.Add("without:" + componentType.FullName);
        _componentPredicates.Add(entity => !entity.Has(componentType));
    }
}

public class FilterBuilder<T> : FilterBuilder
    where T : struct
{
    public FilterBuilder(IWorld world)
        : base(world)
    {
        With<T>();
    }

    public FilterBuilder<T> With<T2>() where T2 : struct
    {
        AddWith(typeof(T2));

        return this;
    }

    public FilterBuilder<T> Without<T2>() where T2 : struct
    {
        AddWithout(typeof(T2));

        return this;
    }

    public IFilter<T> Build()
    {
        var filter = new Filter<T>(
            World,
            IdGenerator.ToString(),
            ComponentPredicates
        );

        return World.CompileFilter(filter);
    }
}

public class FilterBuilder<T1, T2> : FilterBuilder
    where T1 : struct
    where T2 : struct
{
    public FilterBuilder(IWorld world)
        : base(world)
    {
        With<T1>();
        With<T2>();
    }

    public FilterBuilder<T1, T2> With<T3>()
        where T3 : struct
    {
        AddWith(typeof(T3));

        return this;
    }

    public FilterBuilder<T1, T2> Without<T3>() where T3 : struct
    {
        AddWithout(typeof(T3));

        return this;
    }

    public IFilter<T1, T2> Build()
    {
        var filter = new Filter<T1, T2>(
            World,
            IdGenerator.ToString(),
            ComponentPredicates
        );

        return World.CompileFilter(filter);
    }
}

public class FilterBuilder<T1, T2, T3> : FilterBuilder
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    public FilterBuilder(IWorld world)
        : base(world)
    {
        With<T1>();
        With<T2>();
        With<T3>();
    }

    public FilterBuilder<T1, T2, T3> With<T4>()
        where T4 : struct
    {
        AddWith(typeof(T4));

        return this;
    }

    public FilterBuilder<T1, T2, T3> Without<T4>() where T4 : struct
    {
        AddWithout(typeof(T4));

        return this;
    }

    public IFilter<T1, T2, T3> Build()
    {
        var filter = new Filter<T1, T2, T3>(
            World,
            IdGenerator.ToString(),
            ComponentPredicates
        );

        return World.CompileFilter(filter);
    }
}

public class FilterIdGenerator
{
    private readonly SortedSet<string> _idParts = new();

    public void Add(string part)
    {
        _idParts.Add(part);
    }

    public override string ToString()
    {
        var builder = new StringBuilder(_idParts.Count);

        foreach (var idPart in _idParts) {
            builder.Append(idPart);
        }

        return builder.ToString();
    }
}
