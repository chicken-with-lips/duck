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
            All = new ComponentType[]
            {
                typeof(Duck.Ui.Components.UserInterfaceComponent)
            },
            Any = Array.Empty<ComponentType>(),
            None = new ComponentType[]
            {
                typeof(Duck.Ui.Systems.UserInterfaceLoaded)
            },
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
                ref var entityFirstElement = ref chunk.Entity(0);
                ref var userinterfacecomponentFirstElement = ref chunk.GetFirst<Duck.Ui.Components.UserInterfaceComponent>();
                foreach (var entityIndex in chunk)
                {
                    ref readonly var entity = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    ref var cmp = ref Unsafe.Add(ref userinterfacecomponentFirstElement, entityIndex);
                    Run(in entity, in cmp);
                }
            }
        }
    }
}