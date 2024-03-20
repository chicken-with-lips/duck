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

public partial class ApplyLinearDampingSystem : BaseSystem<World, float>
{
    #region Methods

    public ApplyLinearDampingSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in LinearDamping linearDamping, ref LinearVelocity linearVelocity)
    {
        linearVelocity.Value *= MathF.Max(1f - linearDamping.Value * Time.DeltaFrame, 0f);
    }

    #endregion
}
