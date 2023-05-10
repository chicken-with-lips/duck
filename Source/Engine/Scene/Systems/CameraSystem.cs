using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Scene.Systems;

public partial class CameraSystem : BaseSystem<World, float>
{
    private readonly IGraphicsModule _graphicsModule;

    public CameraSystem(World world, IGraphicsModule graphicsModule)
        : base(world)
    {
        _graphicsModule = graphicsModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in CameraComponent camera, in TransformComponent transform)
    {
        if (!camera.IsActive) {
            return;
        }
        
        // FIXME: this should be moved to a view
        
        _graphicsModule.GraphicsDevice.ViewMatrix =
            Matrix4X4.CreateLookAt(
                transform.Position,
                transform.Position + transform.Forward,
                transform.Up
            );
    }
}
