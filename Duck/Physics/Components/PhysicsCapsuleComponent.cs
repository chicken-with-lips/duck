using BepuPhysics;
using BepuPhysics.Collidables;

namespace Duck.Physics.Components
{
    public enum BodyType
    {
        Dynamic,
        Static
    }

    public struct PhysicsBoxComponent
    {
        public float Mass;
    }
}
