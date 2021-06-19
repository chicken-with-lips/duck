using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;

namespace Duck.Rendering.Systems
{
    public class SyncParentTransformConnectionsSystem : SystemBase
    {
        private readonly IFilter<FilamentIdentityComponent, ParentComponent> _filter;
        private readonly TransformManager _transformManager;
        private readonly IWorld _world;

        public SyncParentTransformConnectionsSystem(IWorld world, TransformManager transformManager)
        {
            _world = world;
            _transformManager = transformManager;

            _filter = Filter<FilamentIdentityComponent, ParentComponent>(world)
                .Build();
        }

        public override void Run()
        {
            _transformManager.OpenLocalTransformTransaction();

            foreach (var entityId in _filter.EntityList) {
                var identityComponent = _filter.Get1(entityId);
                var parentComponent = _filter.Get2(entityId);
                var parentIdentityComponent = _world.GetEntity(parentComponent.ParentEntityId).Get<FilamentIdentityComponent>();

                var parentInstance = _transformManager.GetInstance(parentIdentityComponent.Id);
                var localInstance = _transformManager.GetInstance(identityComponent.Id);

                _transformManager.SetParent(localInstance, parentInstance);
            }

            _transformManager.CommitLocalTransformTransaction();
        }
    }
}
