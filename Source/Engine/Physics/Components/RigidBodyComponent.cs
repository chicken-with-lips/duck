using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct RigidBodyComponent
{
    public BodyType Type = BodyType.Static;
    public Lock AxisLock = default;

    public bool IsGravityEnabled = true;
    public bool UseGravityOverride = false;
    public Vector3D<float> GravityOverride = Vector3D<float>.Zero;

    public float Mass = 1;
    public float AngularDamping = 0.05f;
    public float LinearDamping = 0f;

    public RigidBodyComponent()
    {
    }

    public enum BodyType
    {
        Dynamic,
        Kinematic,
        Static
    }

    [Flags]
    public enum Lock
    {
        LinearX = (1 << 0),
        LinearY = (1 << 1),
        LinearZ = (1 << 2),
        AngularX = (1 << 3),
        AngularY = (1 << 4),
        AngularZ = (1 << 5),
    }
}
