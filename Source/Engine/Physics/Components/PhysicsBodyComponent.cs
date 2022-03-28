using BepuPhysics;
using BepuPhysics.Collidables;

namespace Duck.Physics.Components;

public struct PhysicsBodyComponent
{
    public TypedIndex ShapeIndex;
    public BodyHandle BodyHandle;
    public bool IsDynamic;
}
