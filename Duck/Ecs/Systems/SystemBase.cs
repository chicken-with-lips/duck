using Duck.Contracts;
using Duck.Contracts.SceneManagement;

namespace Duck.Ecs.Systems
{
    public abstract class SystemBase : IInitSystem, IRunSystem
    {
        public abstract void Init(IWorld world, IScene scene, IApplication app);
        public abstract void Run();

        public FilterBuilder<T> Filter<T>(IWorld world) where T : struct
        {
            return new FilterBuilder<T>(world);
        }

        public FilterBuilder<T1, T2> Filter<T1, T2>(IWorld world)
            where T1 : struct
            where T2 : struct
        {
            return new FilterBuilder<T1, T2>(world);
        }

        public FilterBuilder<T1, T2, T3> Filter<T1, T2, T3>(IWorld world)
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            return new FilterBuilder<T1, T2, T3>(world);
        }
    }
}
