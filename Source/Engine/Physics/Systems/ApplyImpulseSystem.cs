using System.Runtime.CompilerServices;
using ADyn;
using ADyn.Components;
using ADyn.Shapes;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Physics.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class ApplyImpulseSystem : BaseSystem<World, float>
{
    #region Methods

    public ApplyImpulseSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in MassInv massInv, in InertiaWorldInv inertiaWorldInv, ref LinearVelocity linearVelocity, ref AngularVelocity angularVelocity, ref Impulse impulse)
    {
        linearVelocity.Value += impulse.Value * massInv.Value;
        angularVelocity.Value += Matrix3X3.Multiply(Vector3D.Cross(AVector3.Zero, impulse.Value), inertiaWorldInv.Value);

        impulse.Value = AVector3.Zero;
    }

    #endregion
}
