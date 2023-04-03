using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using ArrayExtensions = CommunityToolkit.HighPerformance.ArrayExtensions;
using Component = Arch.Core.Utils.Component;

namespace Duck.Scene.Systems
{
    public partial class StaticMeshSystem
    {
        private QueryDescription Run_QueryDescription = new QueryDescription
        {
            All = new ComponentType[]
            {
                typeof(Duck.Scene.Components.StaticMeshComponent),
                typeof(Duck.Graphics.Components.TransformComponent)
            },
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
                ref var entityFirstElement = ref chunk.Entity(0);
                ref var staticmeshcomponentFirstElement = ref chunk.GetFirst<Duck.Scene.Components.StaticMeshComponent>();
                ref var transformcomponentFirstElement = ref chunk.GetFirst<Duck.Graphics.Components.TransformComponent>();
                foreach (var entityIndex in chunk)
                {
                    ref readonly var entity = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                    ref var staticmesh = ref Unsafe.Add(ref staticmeshcomponentFirstElement, entityIndex);
                    ref var transform = ref Unsafe.Add(ref transformcomponentFirstElement, entityIndex);
                    Run(in entity, in staticmesh, in transform);
                }
            }
        }
    }
}