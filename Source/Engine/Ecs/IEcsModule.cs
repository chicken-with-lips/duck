namespace Duck.Ecs;

public interface IEcsModule : IModule
{
    public IWorld[] Worlds { get; }

    public IWorld Create();
    public void Destroy(IWorld world);
}
