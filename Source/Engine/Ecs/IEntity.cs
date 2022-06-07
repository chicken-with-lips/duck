using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial interface IEntity
{
    public int Id { get; }

    public ref T Get<T>() where T : struct;

    public void RemoveAll();
    public void Remove(Type componentType);
    public void Remove<T>() where T : struct;

    public bool Has(Type type);
    public bool Has<T>();

    public bool IsAllocated { get; set; }

    public ComponentReference GetComponentReference<T>() where T : struct;
}
