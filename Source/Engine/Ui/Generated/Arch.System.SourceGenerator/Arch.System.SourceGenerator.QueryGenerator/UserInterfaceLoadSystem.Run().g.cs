using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
using Component = Arch.Core.Utils.Component;

namespace Duck.Ui.Systems
{
    public partial class UserInterfaceLoadSystem
    {
        private QueryDescription Run_QueryDescription = new QueryDescription
        {
            All = Array.Empty<ComponentType>(),
            Any = Array.Empty<ComponentType>(),
            None = Array.Empty<ComponentType>(),
            Exclusive = Array.Empty<ComponentType>()
        };
        private bool _Run_Initialized;
        private Query _Run_Query;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunQuery(World world)
        {
            if (!_Run_Initialized)
            {
                _Run_Query = world.Query(in Run_QueryDescription);
                _Run_Initialized = true;
            }

            foreach (ref var chunk in _Run_Query.GetChunkIterator())
            {
                var chunkSize = chunk.Size;
                foreach (var entityIndex in chunk)
                {
                    Run();
                }
            }
        }
    }
}