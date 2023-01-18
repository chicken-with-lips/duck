using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.Ecs;

[AutoSerializable]
public partial interface IWorld : IDisposable
{
    const int NotFound = -1;

    public bool IsActive { get; set; }
    public IFilter[] Filters { get; }
    public ISystemComposition SystemComposition { get; }

    public void BeginFrame();
    public void EndFrame();
    public void Tick();

    public IEntity CreateEntity();
    public IEntity CreateOneShot<TEvent>(ComponentInitializer<TEvent> initializer) where TEvent : struct;

    public void DeleteEntity(IEntity entity);
    public void DeleteEntity(int entityId);

    public ComponentReference AllocateComponent<T>(IEntity entity) where T : struct;
    public void DeallocateComponent(Type componentType, int componentIndex);
    public void DeallocateComponent<T>(int componentIndex) where T : struct;
    public void InternalNotifyComponentAllocated(IEntity entity, ComponentReference componentReference);
    public void InternalNotifyComponentDeallocated(IEntity entity);

    public Type GetTypeFromIndex(int typeIndex);
    public int GetTypeIndexForComponent<T>() where T : struct;
    public int GetTypeIndexForComponent(Type type);
    public ref T GetComponent<T>(int typeIndex, int componentIndex) where T : struct;
    public ref T GetComponent<T>(ComponentReference componentReference) where T : struct;
    public ref T GetComponent<T>(int entityId) where T : struct;
    public bool HasComponent<T>(int entityId) where T : struct;
    public ComponentReference GetComponentReference<T>(int typeIndex, int componentIndex) where T : struct;

    public IEntity GetEntity(int entityId);
    public IEntity[] GetEntitiesByComponent<T>() where T : struct;

    public bool IsEntityAllocated(int entityId);

    public IFilter<T> CompileFilter<T>(IFilter<T> filter) where T : struct;

    public IFilter<T1, T2> CompileFilter<T1, T2>(IFilter<T1, T2> filter)
        where T1 : struct
        where T2 : struct;

    public IFilter<T1, T2, T3> CompileFilter<T1, T2, T3>(IFilter<T1, T2, T3> filter)
        where T1 : struct
        where T2 : struct
        where T3 : struct;
}
