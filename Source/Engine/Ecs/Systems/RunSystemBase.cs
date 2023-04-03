namespace Duck.Ecs.Systems;

public abstract class RunSystemBase<C1> : SystemBase
    where C1 : struct
{
    protected IFilter<C1> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityAddedList) {
            OnAdded(entityId, ref Filter.Get(entityId));
        }

        foreach (var entityId in Filter.EntityRemovedList) {
            OnRemoved(entityId, Filter.Get(entityId));
        }

        foreach (var entityId in Filter.EntityList) {
            OnTick(entityId, ref Filter.Get(entityId));
        }
    }

    public virtual void OnTick(int entityId, ref C1 component)
    {
    }

    public virtual void OnAdded(int entityId, ref C1 component)
    {
    }

    public virtual void OnRemoved(int entityId, in C1 component)
    {
    }
}

public abstract class RunSystemBase<C1, C2> : SystemBase
    where C1 : struct
    where C2 : struct
{
    protected IFilter<C1, C2> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityAddedList) {
            OnAdded(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId)
            );
        }

        foreach (var entityId in Filter.EntityRemovedList) {
            OnRemoved(
                entityId,
                Filter.Get1(entityId),
                Filter.Get2(entityId)
            );
        }

        foreach (var entityId in Filter.EntityList) {
            OnTick(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId)
            );
        }
    }

    public virtual void OnTick(int entityId, ref C1 component1, ref C2 component2)
    {
    }

    public virtual void OnRemoved(int entityId, in C1 component1, in C2 component2)
    {
    }

    public virtual void OnAdded(int entityId, ref C1 component1, ref C2 component2)
    {
    }
}

public abstract class RunSystemBase<C1, C2, C3> : SystemBase
    where C1 : struct
    where C2 : struct
    where C3 : struct
{
    protected IFilter<C1, C2, C3> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityAddedList) {
            OnAdded(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId),
                ref Filter.Get3(entityId)
            );
        }

        foreach (var entityId in Filter.EntityList) {
            OnRemoved(
                entityId,
                Filter.Get1(entityId),
                Filter.Get2(entityId),
                Filter.Get3(entityId)
            );
        }

        foreach (var entityId in Filter.EntityList) {
            OnTick(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId),
                ref Filter.Get3(entityId)
            );
        }
    }

    public virtual void OnTick(int entityId, ref C1 component1, ref C2 component2, ref C3 component3)
    {
    }

    public virtual void OnAdded(int entityId, ref C1 component1, ref C2 component2, ref C3 component3)
    {
    }

    public virtual void OnRemoved(int entityId, in C1 component1, in C2 component2, in C3 component3)
    {
    }
}
