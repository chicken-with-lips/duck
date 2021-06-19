using Duck.Ecs;

namespace Duck.Physics
{
    public interface IPhysicsSubsystem : IApplicationSubsystem
    {
        public IPhysicsWorld GetOrCreatePhysicsWorld(IWorld world);
    }
}
