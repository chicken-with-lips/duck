namespace Duck.Ecs;

public interface IFilter
{
    public string Id { get; }
    public int[] EntityList { get; }
    public int[] EntityAddedList { get; }
    public int[] EntityRemovedList { get; }

    public FilterComponentPredicate[] ComponentPredicates { get; }
    public void QueueAddition(IEntity entity);
    public void QueueRemoval(IEntity entity);

    public void SwapDirtyBuffers();
}

public interface IFilter<T> : IFilter where T : struct
{
    public IEntity GetEntity(int entityId);

    public ref T Get(int entityId);

    public ref T GetAdded(int entityId);
}

public interface IFilter<T1, T2> : IFilter
    where T1 : struct
    where T2 : struct
{
    public IEntity GetEntity(int entityId);

    public ref T1 Get1(int entityId);

    public ref T2 Get2(int entityId);
}

public interface IFilter<T1, T2, T3> : IFilter
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    public IEntity GetEntity(int entityId);

    public ref T1 Get1(int entityId);

    public ref T2 Get2(int entityId);

    public ref T3 Get3(int entityId);
}

public delegate bool FilterComponentPredicate(IEntity entity);
