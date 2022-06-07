using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial class Entity : IEntity
{
    public IWorld World => _world;

    private int[] _components;
    private bool _isAllocated;
    private IWorld _world;

    public Entity(IWorld world, int index)
    {
        Id = index;
        _world = world;

        _components = new int[100];

        for (var i = 0; i < _components.Length; i++) {
            _components[i] = -1;
        }
    }

    #region IEntity

    public int Id { get; }

    public ref T Get<T>() where T : struct
    {
        if (!IsAllocated) {
            throw new Exception("FIXME: errors");
        }

        var typeIndex = World.GetTypeIndexForComponent<T>();

        if (_components[typeIndex] != IWorld.NotFound) {
            return ref World.GetComponent<T>(typeIndex, _components[typeIndex]);
        }

        var componentReference = World.AllocateComponent<T>(this);
        _components[componentReference.TypeIndex] = componentReference.ComponentIndex;

        World.InternalNotifyComponentAllocated(componentReference);

        return ref World.GetComponent<T>(typeIndex, componentReference.ComponentIndex);
    }

    public void RemoveAll()
    {
        for (var typeIndex = 0; typeIndex < _components.Length; typeIndex++) {
            if (_components[typeIndex] == IWorld.NotFound) {
                continue;
            }

            Remove(
                World.GetTypeFromIndex(typeIndex)
            );
        }
    }

    public void Remove(Type componentType)
    {
        var typeIndex = World.GetTypeIndexForComponent(componentType);

        if (_components[typeIndex] == IWorld.NotFound) {
            return;
        }

        var cmpIndex = _components[typeIndex];

        _components[typeIndex] = IWorld.NotFound;

        World.DeallocateComponent(componentType, cmpIndex);
        World.InternalNotifyComponentDeallocated(this);
    }

    public void Remove<T>() where T : struct
    {
        Remove(typeof(T));
    }

    public ComponentReference GetComponentReference<T>() where T : struct
    {
        var typeIndex = World.GetTypeIndexForComponent<T>();

        if (_components[typeIndex] != IWorld.NotFound) {
            return World.GetComponentReference<T>(typeIndex, _components[typeIndex]);
        }

        throw new Exception("Component reference not found");
    }

    public bool Has(Type type)
    {
        var typeIndex = World.GetTypeIndexForComponent(type);

        return typeIndex != IWorld.NotFound && _components[typeIndex] != IWorld.NotFound;
    }

    public bool Has<T>()
    {
        return Has(typeof(T));
    }

    public bool IsAllocated {
        get { return _isAllocated; }
        set { _isAllocated = value; }
    }

    #endregion
}
