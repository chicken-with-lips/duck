using Duck.Ecs;

namespace Duck.Contracts.Physics
{
    public interface IPhysicsSubsystem : IApplicationSubsystem
    {
        public IPhysicsWorld? GetPhysicsWorld(IWorld world);
    }
}
