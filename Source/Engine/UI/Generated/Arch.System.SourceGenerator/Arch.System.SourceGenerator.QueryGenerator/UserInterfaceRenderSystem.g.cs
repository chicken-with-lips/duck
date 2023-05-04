using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace Duck.Ui.Systems
{
    public partial class UserInterfaceRenderSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Update(in float single)
        {
            RunQuery(World);
        }
    }
}