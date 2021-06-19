using Duck.Contracts;
using Duck.Contracts.SceneManagement;

namespace Duck.Ecs
{
    public interface ISystem
    {
    }

    public interface IInitSystem : ISystem
    {
        public void Init(IWorld world, IScene scene, IApplication app);
    }

    public interface IRunSystem : ISystem
    {
        public void Run();
    }
}
