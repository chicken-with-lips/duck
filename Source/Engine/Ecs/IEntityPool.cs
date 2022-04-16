using Duck.Serialization;

namespace Duck.Ecs;

[AutoSerializable]
public partial interface IEntityPool
{
    public int Count { get; }
    public IEntity Allocate();
    public IEntity Get(int entityId);
}
