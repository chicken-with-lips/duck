using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;

namespace Duck.Graphics.Systems;

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
        if (staticMesh.Mesh == AssetReference<StaticMesh>.Null) {
            return;
        }

        World.Add<RuntimeStaticMeshComponent>(entity);

        var mesh = _contentModule.LoadImmediate(staticMesh.Mesh) as IRenderable;

        // FIXME: what was I trying to do here?
        ref var runtimeData = ref World.Get<RuntimeStaticMeshComponent>(entity);
        runtimeData.InstanceId = mesh.RenderObject.CreateInstance().Id;
    }
}
