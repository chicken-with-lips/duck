using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Scene.Components;

namespace Duck.Scene.Systems;

public partial class StaticMeshSystem : BaseSystem<World, float>
{
    private readonly IContentModule _contentModule;

    public StaticMeshSystem(World world, IContentModule contentModule)
        : base(world)
    {
        _contentModule = contentModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in StaticMeshComponent staticMesh, in TransformComponent transform)
    {
        if (staticMesh.Mesh == null) {
            return;
        }

        if (!entity.Has<RuntimeStaticMeshComponent>()) {
            entity.Add<RuntimeStaticMeshComponent>();

            var mesh = _contentModule.LoadImmediate<StaticMesh>(staticMesh.Mesh) as IRenderable;

            // FIXME: what was I trying to do here?
            ref var runtimeData = ref entity.Get<RuntimeStaticMeshComponent>();
            runtimeData.InstanceId = mesh.RenderObject.CreateInstance().Id;
        }
    }
}
