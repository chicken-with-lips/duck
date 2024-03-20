using Arch.Core;
using Duck.Platform;

namespace Duck.Physics;

public interface IPhysicsModule : IModule
{
    public IPhysicsScene GetOrCreatePhysicsScene(World world);
    public void DestroyPhysicsSceneForWorld(World world);
}
