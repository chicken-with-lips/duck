using Duck.Contracts.SceneManagement;
using Duck.Ecs;

namespace Duck.Contracts
{
    public interface IApplication
    {
        public bool Init();

        public void Run();

        public void Shutdown();

        public ISystemComposition CreateDefaultSystemComposition(IScene scene);

        public T GetSubsystem<T>() where T : IApplicationSubsystem;
    }

    public interface IApplicationSubsystem
    {
    }

    public interface IApplicationInitializableSubsystem : IApplicationSubsystem
    {
        public bool Init();
    }

    public interface IApplicationTickableSubsystem : IApplicationSubsystem
    {
        public void Tick();
    }

    public interface IApplicationPreTickableSubsystem : IApplicationSubsystem
    {
        public void PreTick();
    }

    public interface IApplicationPostTickableSubsystem : IApplicationSubsystem
    {
        public void PostTick();
    }
}
