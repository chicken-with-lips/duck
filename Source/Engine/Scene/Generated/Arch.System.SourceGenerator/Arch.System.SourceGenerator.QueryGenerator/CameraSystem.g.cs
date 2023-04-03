using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace Duck.Scene.Systems
{
    public partial class CameraSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Update(in float single)
        {
            RunQuery(World);
        }
    }
}