namespace Duck.Ecs;

public interface IEntityPool
{
    public int Count { get; }
    public IEntity Allocate();
    public IEntity Get(int entityId);
}
