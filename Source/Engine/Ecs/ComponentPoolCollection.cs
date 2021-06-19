namespace Duck.Ecs;

public class ComponentPoolCollection : IComponentPoolCollection
{
    private IComponentPoolBase[] _componentPools;
    private int _nextComponentPoolIndex;
    private int _initialPoolSize;

    /// <summary>
    /// Used for serialization.
    /// </summary>
#pragma warning disable 8618
    internal ComponentPoolCollection()
#pragma warning restore 8618
    {
    }

    public ComponentPoolCollection(int poolCount, int initialPoolSize)
    {
        _componentPools = new IComponentPoolBase[poolCount];
        _initialPoolSize = initialPoolSize;
    }

    #region IComponentPoolCollection

    public ComponentReference GetComponentReference<T>(int typeIndex, int componentIndex) where T : struct
    {
        if (typeIndex >= _componentPools.Length) {
            throw new Exception("Tried to get unknown type index");
        }

        return _componentPools[typeIndex].GetComponentReference(componentIndex);
    }

    public ref T GetComponent<T>(int typeIndex, int componentIndex) where T : struct
    {
        if (typeIndex >= _componentPools.Length) {
            throw new Exception("Tried to get unknown type index");
        }

        return ref ((IComponentPool<T>)_componentPools[typeIndex]).Get(componentIndex);
    }

    public ComponentReference AllocateComponent<T>(IEntity entity) where T : struct
    {
        return GetOrAllocateComponentPool<T>().Allocate(entity.Id);
    }

    public void DeallocateComponent<T>(int componentIndex) where T : struct
    {
        GetOrAllocateComponentPool<T>().Deallocate(componentIndex);
    }

    public int GetTypeIndexForComponent<T>() where T : struct
    {
        return GetOrAllocateComponentPool<T>().TypeIndex;
    }

    public int GetTypeIndexForComponent(Type type)
    {
        return GetComponentPool(type)?.TypeIndex ?? -1;
    }

    #endregion


    private IComponentPool<T> GetOrAllocateComponentPool<T>() where T : struct
    {
        var pool = GetComponentPool<T>();

        if (null != pool) {
            return pool;
        }

        // FIXME: resize+gc
        if (_nextComponentPoolIndex == _componentPools.Length) {
            throw new Exception("World ran out of component pools");
        }

        var poolIndex = _nextComponentPoolIndex;

        _componentPools[poolIndex] = new ComponentPool<T>(
            _nextComponentPoolIndex,
            _initialPoolSize
        );

        _nextComponentPoolIndex += 1;

        return (IComponentPool<T>)_componentPools[poolIndex];
    }

    private IComponentPool<T>? GetComponentPool<T>() where T : struct
    {
        var pool = GetComponentPool(typeof(T));

        return pool as IComponentPool<T>;
    }

    private IComponentPoolBase? GetComponentPool(Type type)
    {
        foreach (var pool in _componentPools) {
            if (pool?.AppliesTo(type) == true) {
                return pool;
            }
        }

        return null;
    }
}
