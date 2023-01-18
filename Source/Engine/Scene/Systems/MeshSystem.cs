using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Scene.Components;

namespace Duck.Scene.Systems;

public class MeshLoadSystem : RunSystemBase<MeshComponent, TransformComponent>
{
    private readonly IContentModule _contentModule;
    private readonly IGraphicsModule _graphicsModule;

    public MeshLoadSystem(IWorld world, IContentModule contentModule, IGraphicsModule graphicsModule)
    {
        _contentModule = contentModule;
        _graphicsModule = graphicsModule;

        Filter = Filter<MeshComponent, TransformComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref MeshComponent meshComponent, ref TransformComponent transformComponent)
    {
        if (meshComponent.Mesh == null) {
            return;
        }

        var mesh = _contentModule.LoadImmediate(meshComponent.Mesh);

        if (mesh is IRenderable renderable) {
            var instance = _graphicsModule.GraphicsDevice.CreateRenderObjectInstance(renderable.RenderObject);

            ref var instanceComponent = ref Filter.GetEntity(entityId).Get<RenderableInstanceComponent>();
            instanceComponent.Id = instance.Id;
        }
    }
}
