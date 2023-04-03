using Arch.Core;

namespace Duck.Physics;

public interface IPhysicsModule : IModule
{
    public IPhysicsWorld GetOrCreatePhysicsWorld(World world);
}
