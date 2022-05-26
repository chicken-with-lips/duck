using Silk.NET.Maths;

namespace Duck.Physics;

public interface IPhysicsWorld
{
    public void Step(float timeStep);
    Vector3D<float> Gravity { get; set; }
}
