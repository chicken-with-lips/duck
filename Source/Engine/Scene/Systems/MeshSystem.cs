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
            var mesh = _contentModule.LoadImmediate(_filter.Get1(entityId).Mesh);

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

public class MeshRenderSystem : SystemBase
{
    private readonly IFilter<RenderableInstanceComponent, TransformComponent> _filter;
    private readonly IGraphicsModule _graphicsModule;

    public MeshRenderSystem(IScene scene, IGraphicsModule graphicsModule)
    {
        _graphicsModule = graphicsModule;

        _filter = Filter<RenderableInstanceComponent, TransformComponent>(scene.World)
            .Build();
    }

    public override void Run()
    {
        foreach (var entityId in _filter.EntityList) {
            var instanceComponent = _filter.Get1(entityId);
            var transformComponent = _filter.Get2(entityId);

            var renderObjectInstance = _graphicsModule.GraphicsDevice.GetRenderObjectInstance(instanceComponent.Id);
            renderObjectInstance
                .SetParameter("WorldPosition",
                    Matrix4X4.CreateScale(transformComponent.Scale)
                    * Matrix4X4.CreateFromQuaternion(transformComponent.Rotation)
                    * Matrix4X4.CreateTranslation(transformComponent.Position)
                );
        }
    }
}
