namespace Duck.Ecs;

public interface ISystem
{
}

public interface IRunSystem : ISystem
{
    public void Run();
}
