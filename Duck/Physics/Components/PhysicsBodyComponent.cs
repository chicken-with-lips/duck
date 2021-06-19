using BepuPhysics;
using BepuPhysics.Collidables;

namespace Duck.Physics.Components
{
    public struct PhysicsBodyComponent
    {
        public TypedIndex CachedShapeIndex;
        public BodyHandle CachedBodyHandle;
    }
}
