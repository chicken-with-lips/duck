namespace Duck.Ecs;

public interface ISystemComposition
{
    public IWorld World { get; }

    public ISystemComposition Add(ISystem system);
    public void Init();
    public void Tick();
}
