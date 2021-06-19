using Duck.Contracts;
using Duck.Contracts.SceneManagement;

namespace Duck.Ecs
{
    public interface ISystemComposition
    {
        public IWorld World { get; }

        public ISystemComposition Add(ISystem system);
        public void Init(IScene scene, IApplication application);
        public void Tick();
    }
}
