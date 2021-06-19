using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;
using Scene = Duck.SceneManagement.Scene;

namespace Duck.FilamentBridge.Systems
{
    public class GenerateFilamentIdentitySystem : SystemBase
    {
        private readonly IFilter<TransformComponent> _filter;
        private readonly Scene _scene;
        private readonly TransformManager _transformManager;

        public GenerateFilamentIdentitySystem(IWorld world, Scene scene, TransformManager transformManager)
        {
            _scene = scene;
            _transformManager = transformManager;

            _filter = Filter<TransformComponent>(world)
                .Without<FilamentIdentityComponent>()
                .Build();
        }

        public override void Run()
        {
            foreach (var entityId in _filter.EntityAddedList) {
                var entity = _filter.GetEntity(entityId);

                ref var identityComponent = ref entity.Get<FilamentIdentityComponent>();
                identityComponent.Id = EntityManager.Create();

                _transformManager.Create(identityComponent.Id);
                _scene.RenderingScene.AddEntity(identityComponent.Id);
            }
        }
    }
}
