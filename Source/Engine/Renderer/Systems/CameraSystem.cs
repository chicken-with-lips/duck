using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Renderer.Systems;

public partial class CameraSystem : BaseSystem<World, float>
{
    private readonly IRendererModule _renderModule;

    public CameraSystem(World world, IRendererModule renderModule)
        : base(world)
    {
        _renderModule = renderModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in TransformComponent transform, in CameraComponent camera)
    {
        if (!camera.IsActive) {
            return;
        }

        if (null != _renderModule.GameView) {
            _renderModule.GameView.Camera = entity.Reference();
        }

        foreach (var view in _renderModule.Views) {
            view.Camera = entity.Reference();
        }
    }
}
