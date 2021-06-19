using Duck.Ecs;

namespace Duck.Contracts.SceneManagement
{
    public interface IScene
    {
        public IWorld World { get; }
    }
}
