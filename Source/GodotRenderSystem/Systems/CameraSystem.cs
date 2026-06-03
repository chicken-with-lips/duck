using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Duck.Scene;
using Duck.Scene.Components;
using Duck.Platform;
using Duck.RenderSystem.Godot.Components;
using Duck.RenderSystem.Godot.Extensions;
using Godot;
using Silk.NET.Maths;

namespace Duck.RenderSystem.Godot.Systems;

public partial class CameraSystem : BaseSystem<World, FrameTimer>, IBufferedSystem
{
    public CommandBuffer? CommandBuffer { get; set; }

    public CameraSystem(World world) : base(world)
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        World.SubscribeComponentRemoved<Camera>(OnCameraRemoved);
    }

    private void OnCameraRemoved(in Entity entity, ref Camera cmp)
    {
        if (World.Has<GodotCameraHandle>(entity)) {
            World.Remove<GodotCameraHandle>(entity);
        }
    }

    [Query]
    [None<GodotCameraHandle>]
    private void CreateCameraInGodot(in Entity entity, in Camera camera)
    {
        CommandBuffer?.Add(entity, new GodotCameraHandle {
            Value = RenderingServer.CameraCreate(),
        });
    }

    private Vector3D<float> _position = new(0, 0, -5);

    private float _time = 0;
    private bool flip;
    
    [Query]
    private void SyncCameraTransformInGodot([Data] in FrameTimer timer, in GodotCameraHandle cameraHandle,
        ref Position position)
    {
        // _position -= ((Vector3D<float>.UnitZ * 1.0f) * timer.Delta);
            // Console.WriteLine(_position + " -> " + timer.Delta);
            if (_time > 1) {
                if (flip) {
                    GodotRenderModule.mat.AlbedoColor = new Color(1, 0, 0);
                } else {
                    GodotRenderModule.mat.AlbedoColor = new Color(0, 0, 1);
                }

                flip = !flip;
                _time = 0;
            } else {
                _time += timer.Delta;
            }
            
            

            RenderingServer.InstanceSetTransform(
            GodotRenderModule.cubeRid, new Transform3D(
                Basis.Identity,
                _position.ToGodot()));


        RenderingServer.CameraSetTransform(cameraHandle.Value, new Transform3D(
            Basis.Identity,
            position.Value.ToGodot())
        );
    }
}