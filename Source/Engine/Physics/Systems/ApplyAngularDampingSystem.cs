using System.Runtime.CompilerServices;
using ADyn;
using ADyn.Components;
using ADyn.Shapes;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Physics.Components;
using Duck.Platform;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public partial class ApplyAngularDampingSystem : BaseSystem<World, float>
{
    #region Methods

    public ApplyAngularDampingSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in MassInv massInv, in AngularDamping angularDamping, ref AngularVelocity angularVelocity)
    {
        angularVelocity.Value *= MathF.Max(1f - angularDamping.Value * Time.DeltaFrame, 0f);
    }

    #endregion
}
