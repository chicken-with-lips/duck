using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Content;
using Duck.Renderer.Components;
using Duck.Renderer.Device;
using Duck.Renderer.Mesh;

namespace Duck.Renderer.Systems;

public partial class LoadStaticMeshSystem : BaseSystem<World, float>
{
    private readonly IContentModule _contentModule;

    public LoadStaticMeshSystem(World world, IContentModule contentModule)
        : base(world)
    {
        _contentModule = contentModule;
    }

    [Query]
    [None<RuntimeStaticMeshComponent>]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Load(in Entity entity, in StaticMeshComponent staticMesh)
    {
        if (staticMesh.Mesh == null) {
            return;
        }

        entity.Add<RuntimeStaticMeshComponent>();

        var mesh = _contentModule.LoadImmediate<StaticMesh>(staticMesh.Mesh) as IRenderable;

        // FIXME: what was I trying to do here?
        ref var runtimeData = ref entity.Get<RuntimeStaticMeshComponent>();
        runtimeData.InstanceId = mesh.RenderObject.CreateInstance().Id;
    }
}
