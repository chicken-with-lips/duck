using Arch.Core;
using Duck.Platform;

namespace Duck.Physics;

public interface IPhysicsModule : IModule
{
    public IPhysicsWorld GetOrCreatePhysicsWorld(World world);
}
