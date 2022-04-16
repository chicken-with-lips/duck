using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial interface IComponentPoolCollection
{
    public ComponentReference GetComponentReference<T>(int typeIndex, int componentIndex) where T : struct;
    public ref T GetComponent<T>(int typeIndex, int componentIndex) where T : struct;

    public ComponentReference AllocateComponent<T>(IEntity entity) where T : struct;
    public void DeallocateComponent<T>(int componentIndex) where T : struct;

    public int GetTypeIndexForComponent<T>() where T : struct;
    public int GetTypeIndexForComponent(Type type);
}
