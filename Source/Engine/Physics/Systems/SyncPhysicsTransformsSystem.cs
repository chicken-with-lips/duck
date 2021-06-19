using BepuPhysics;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;

namespace Duck.Physics.Systems
{
    public class SyncPhysicsTransformsSystem : SystemBase
    {
        #region Members

        private readonly IFilter<PhysicsBodyComponent, TransformComponent> _filter;
        private readonly PhysicsWorld _physicsWorld;

        #endregion

        #region Methods

        public SyncPhysicsTransformsSystem(IWorld world, IPhysicsSubsystem physicsSubsystem)
        {
            _physicsWorld = (PhysicsWorld) physicsSubsystem.GetOrCreatePhysicsWorld(world);

            _filter = Filter<PhysicsBodyComponent, TransformComponent>(world)
                .Build();
        }

        public override void Run()
        {
            var simulation = _physicsWorld.Simulation;

            foreach (var entityId in _filter.EntityList) {
                PhysicsBodyComponent bodyComponent = _filter.Get1(entityId);
                ref TransformComponent transform = ref _filter.Get2(entityId);

                if (!bodyComponent.IsDynamic) {
                    continue;
                }

                BodyReference body = simulation.Bodies.GetBodyReference(bodyComponent.BodyHandle);

                if (!body.Exists) {
                    continue;
                }

                if (transform.IsTranslationDirty) {
                    body.Pose.Position = transform.Translation;
                } else {
                    transform.Translation = body.Pose.Position;
                }

                if (transform.IsRotationDirty) {
                    body.Pose.Orientation = transform.Rotation;
                } else {
                    transform.Rotation = body.Pose.Orientation;
                }

                transform.ClearDirtyFlags();
            }
        }

        #endregion
    }
}
