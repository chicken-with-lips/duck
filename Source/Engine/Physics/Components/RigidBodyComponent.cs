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

    public float AngularDamping = 0.05f;
    public float LinearDamping = 0f;

    public float MaxLinearVelocity { get; set; } = -1f;

    public Vector3D<float> LinearVelocity { get; internal set; } = default;
    public Vector3D<float> AngularVelocity { get; internal set; } = default;


    public Vector3D<float> InertiaTensor {
        get => _inertiaTensor;
        set {
            _inertiaTensor = value;
            _inertiaTensorDirty = true;
        }
    }

    public Quaternion<float> InertiaTensorRotation { get; internal set; } = default;

    public bool IsInertiaTensorDirty => _inertiaTensorDirty;

    public Vector3D<float> MassSpaceInvInertiaTensor { get; internal set; } = default;

    internal Vector3D<float> AccumulatedAccelerationForce = default;
    internal Vector3D<float> AccumulatedForceForce = default;
    internal Vector3D<float> AccumulatedImpulseForce = default;
    internal Vector3D<float> AccumulatedVelocityChangeForce = default;

    internal Vector3D<float> AccumulatedAccelerationTorque = default;
    internal Vector3D<float> AccumulatedForceTorque = default;
    internal Vector3D<float> AccumulatedImpulseTorque = default;
    internal Vector3D<float> AccumulatedVelocityChangeTorque = default;

    private Vector3D<float> _inertiaTensor = default;
    private bool _inertiaTensorDirty = default;

    public RigidBodyComponent()
    {
    }

    public void AddForce(Vector3D<float> force, ForceMode mode)
    {
        switch (mode) {
            case ForceMode.Acceleration:
                AccumulatedAccelerationForce += force;
                break;
            case ForceMode.Force:
                AccumulatedForceForce += force;
                break;
            case ForceMode.Impulse:
                AccumulatedImpulseForce += force;
                break;
            case ForceMode.VelocityChange:
                AccumulatedVelocityChangeForce += force;
                break;
        }
    }

    public void AddTorque(Vector3D<float> force, ForceMode mode)
    {
        switch (mode) {
            case ForceMode.Acceleration:
                AccumulatedAccelerationTorque += force;
                break;
            case ForceMode.Force:
                AccumulatedForceTorque += force;
                break;
            case ForceMode.Impulse:
                AccumulatedImpulseTorque += force;
                break;
            case ForceMode.VelocityChange:
                AccumulatedVelocityChangeTorque += force;
                break;
        }
    }

    public void ClearForces()
    {
        AccumulatedAccelerationForce = default;
        AccumulatedForceForce = default;
        AccumulatedImpulseForce = default;
        AccumulatedVelocityChangeForce = default;

        AccumulatedAccelerationTorque = default;
        AccumulatedForceTorque = default;
        AccumulatedImpulseTorque = default;
        AccumulatedVelocityChangeTorque = default;
    }

    internal void ClearDirtyFlags()
    {
        _inertiaTensorDirty = false;
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

    public enum ForceMode
    {
        /// <summary>Parameter has unit of mass * distance / time^2, i.e. a force.</summary>
        Force,

        /// <summary>Parameter has unit of mass * distance / time.</summary>
        Impulse,

        /// <summary>Parameter has unit of distance / time, i.e. the effect is mass independent: a velocity change.</summary>
        VelocityChange,

        /// <summary>Parameter has unit of distance/ time^2, i.e. an acceleration. It gets treated just like a force except the mass is not divided out before integration.</summary>
        Acceleration,
    }
}
