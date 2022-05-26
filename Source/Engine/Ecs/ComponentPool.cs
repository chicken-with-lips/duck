using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial class ComponentPool<T> : IComponentPool<T> where T : struct
{
    private ComponentData<T>[] _components;
    private int _nextComponentIndex = 0;

    /// <summary>
    /// Used for serialization.
    /// </summary>
#pragma warning disable 8618
    internal ComponentPool()
#pragma warning restore 8618
    {
    }

    public ComponentPool(int typeIndex, int initialSize)
    {
        TypeIndex = typeIndex;

        _components = new ComponentData<T>[initialSize];

        for (var i = 0; i < _components.Length; i++) {
            _components[i].EntityId = -1;
        }
    }

    #region IComponentPool

    public int TypeIndex { get; }

    public int ComponentCount => _nextComponentIndex;

    public bool AppliesTo(Type type)
    {
        return type == typeof(T);
    }

    public ComponentReference Allocate(int entityId)
    {
        if ((_nextComponentIndex + 1) == _components.Length) {
            throw new Exception("ComponentPool ran out of components");
        }

        var cmpIndex = _nextComponentIndex;
        _nextComponentIndex += 1;

        _components[cmpIndex].EntityId = entityId;
        _components[cmpIndex].Data = Activator.CreateInstance<T>();

        return new ComponentReference() {
            TypeIndex = TypeIndex,
            ComponentIndex = cmpIndex,
            EntityId = _components[cmpIndex].EntityId
        };
    }

    public void Deallocate(int componentIndex)
    {
        // TODO: reuse deallocated components
        _components[componentIndex].EntityId = -1;
    }

    public ref T Get(int componentIndex)
    {
        if (componentIndex >= _components.Length) {
            throw new Exception("Out of bounds trying to get component");
        }

        return ref _components[componentIndex].Data;
    }

    public ComponentReference GetComponentReference(int componentIndex)
    {
        if (componentIndex >= _components.Length) {
            throw new Exception("Out of bounds trying to get component");
        }

        return new ComponentReference() {
            TypeIndex = TypeIndex,
            ComponentIndex = componentIndex,
            EntityId = _components[componentIndex].EntityId
        };
    }

    #endregion
}
