using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Scene.Systems;

public class MeshLoadSystem : SystemBase
{
    private readonly IFilter<MeshComponent, TransformComponent> _filter;
    private readonly IScene _scene;
    private readonly IContentModule _contentModule;
    private readonly IGraphicsModule _graphicsModule;

    public MeshLoadSystem(IScene scene, IContentModule contentModule, IGraphicsModule graphicsModule)
    {
        _scene = scene;
        _contentModule = contentModule;
        _graphicsModule = graphicsModule;

        _filter = Filter<MeshComponent, TransformComponent>(scene.World)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityAddedList) {
            var cmp = _filter.Get1(entityId);

            if (cmp.Mesh == null) {
                continue;
            }

            var mesh = _contentModule.LoadImmediate(cmp.Mesh);

            if (mesh is IRenderable renderable) {
                var instance = _graphicsModule.GraphicsDevice.CreateRenderObjectInstance(renderable.RenderObject);

                ref var instanceComponent = ref _filter.GetEntity(entityId).Get<RenderableInstanceComponent>();
                instanceComponent.Id = instance.Id;

                _scene.AddRenderable(entityId);
            }
        }

        foreach (var entityId in _filter.EntityRemovedList) {
            _scene.RemoveRenderable(entityId);
        }
    }
}
