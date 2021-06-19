using System;

namespace Duck.Ecs
{
    public interface IComponentPoolBase
    {
        public int TypeIndex { get; }
        public int ComponentCount { get; }
        public bool AppliesTo(Type type);
        public ComponentReference GetComponentReference(int componentIndex);

        public object X();
    }

    public interface IComponentPool<T> : IComponentPoolBase where T : struct
    {
        public ComponentReference Allocate(int entityId);
        public void Deallocate(int componentIndex);

        public ref T Get(int componentIndex);
    }


    [Serializable]
    public struct ComponentData<T>
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
}
