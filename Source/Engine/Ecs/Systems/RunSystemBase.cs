namespace Duck.Ecs.Systems;

public abstract class RunSystemBase<C1> : SystemBase
    where C1 : struct
{
    protected IFilter<C1> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityList) {
            RunEntity(entityId, ref Filter.Get(entityId));
        }
    }

    public abstract void RunEntity(int entityId, ref C1 component);
}

public abstract class RunSystemBase<C1, C2> : SystemBase
    where C1 : struct
    where C2 : struct
{
    protected IFilter<C1, C2> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityList) {
            RunEntity(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId)
            );
        }
    }

    public abstract void RunEntity(int entityId, ref C1 component1, ref C2 component2);
}

public abstract class RunSystemBase<C1, C2, C3> : SystemBase
    where C1 : struct
    where C2 : struct
    where C3 : struct
{
    protected IFilter<C1, C2, C3> Filter { get; init; }

    public override void Run()
    {
        foreach (var entityId in Filter.EntityList) {
            RunEntity(
                entityId,
                ref Filter.Get1(entityId),
                ref Filter.Get2(entityId),
                ref Filter.Get3(entityId)
            );
        }
    }

    public abstract void RunEntity(int entityId, ref C1 component1, ref C2 component2, ref C3 component3);
}
