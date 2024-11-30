using System.Runtime.CompilerServices;
using ADyn.Components;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class ApplyTorqueSystem : BaseSystem<World, float>
{
    #region Methods

    public ApplyTorqueSystem(World world)
        : base(world)
    {
    }

    [Query]
    [All<RigidBodyTag>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in InertiaWorldInv inertiaWorldInv, ref AngularVelocity angularVelocity, ref Torque torque)
    {
        angularVelocity.Value += Matrix3X3.Multiply(torque.Value, inertiaWorldInv.Value);

        torque.Value = AVector3.Zero;
    }

    #endregion
}
