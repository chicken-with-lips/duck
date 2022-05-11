using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Scene.Systems;

public class CameraSystem : SystemBase
{
    private readonly IFilter<CameraComponent, TransformComponent> _filter;
    private readonly IGraphicsModule _graphicsModule;

    public CameraSystem(IScene scene, IGraphicsModule graphicsModule)
    {
        _graphicsModule = graphicsModule;

        _filter = Filter<CameraComponent, TransformComponent>(scene.World)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {
            var cameraComponent = _filter.Get1(entityId);
            var transformComponent = _filter.Get2(entityId);

            if (!cameraComponent.IsActive) {
                continue;
            }

            _graphicsModule.GraphicsDevice.ViewMatrix =
                Matrix4X4.CreateLookAt(
                    transformComponent.Translation,
                    transformComponent.Translation + transformComponent.Forward,
                    transformComponent.Up
                );
        }
    }
}
