using Duck.Contracts;
using Duck.Contracts.SceneManagement;
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
        private IFilter<TransformComponent>? _filter;
        private IWorld? _world;
        private Scene? _scene;
        private TransformManager? _transformManager;

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _world = world;
            _scene = scene as Scene;
            _transformManager = app.GetSubsystem<RenderingSubsystem>().Engine.TransformManager;

            _filter = Filter<TransformComponent>(world)
                .Without<FilamentIdentityComponent>()
                .Build();
        }

        public override void Run()
        {
            foreach (var entity in _filter.EntityAddedList) {
                var ent = _filter.GetEntity(entity);

                ref var cmp = ref ent.Get<FilamentIdentityComponent>();
                cmp.Id = EntityManager.Create();

                _scene.RenderingScene.AddEntity(cmp.Id);
                _transformManager.Create(cmp.Id);
            }
        }
    }
}
