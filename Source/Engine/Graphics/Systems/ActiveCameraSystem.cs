using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Graphics.Components;

namespace Duck.Graphics.Systems;

public partial class ActiveCameraSystem : BaseSystem<World, float>
{
    private readonly IScene _scene;
    private readonly IRendererModule _renderModule;

    public ActiveCameraSystem(IScene scene, World world, IRendererModule renderModule)
        : base(world)
    {
        _scene = scene;
        _renderModule = renderModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in CameraComponent camera)
    {
        if (null == _renderModule.PrimaryView) {
            return;
        }

        if (!camera.IsActive || _scene != _renderModule.PrimaryView.Scene) {
            return;
        }

        _renderModule.PrimaryView.Camera = World.Reference(entity);

        // foreach (var view in _renderModule.Views) {
        // view.Camera = entity.Reference();
        // }
    }
}
