namespace Duck.Ecs;

public interface IWorldModule : IModule
{
    public IWorld[] Worlds { get; }

    public IWorld Create();
}
