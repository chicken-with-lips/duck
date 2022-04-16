namespace Duck.Ecs;

public interface IWorldSubsystem : IApplicationSubsystem
{
    public IWorld[] Worlds { get; }

    public IWorld Create();
}
