using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial interface IComponentPoolBase
{
    public int TypeIndex { get; }
    public int ComponentCount { get; }
    public bool AppliesTo(Type type);
    public ComponentReference GetComponentReference(int componentIndex);
}

[AutoSerializable]
public partial interface IComponentPool<T> : IComponentPoolBase where T : struct
{
    public ComponentReference Allocate(int entityId);
    public void Deallocate(int componentIndex);

    public ref T Get(int componentIndex);
}

public struct ComponentData<T> where T : struct
{
    public int EntityId;
    public T Data;
}

public struct ComponentReference
{
    public int EntityId;
    public int TypeIndex;
    public int ComponentIndex;
}
