namespace Duck.Ecs;

public interface IEcsModule : IModule
{
    public IWorld[] Worlds { get; }

    public IWorld Create();
}
