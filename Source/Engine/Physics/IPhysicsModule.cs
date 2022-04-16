using Duck.Ecs;

namespace Duck.Physics;

public interface IPhysicsModule : IModule
{
    public IPhysicsWorld GetOrCreatePhysicsWorld(IWorld world);
}
