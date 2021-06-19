using System.Numerics;
using BepuPhysics;
using Duck.Contracts;
using Duck.Contracts.Physics;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.SceneManagement.Components;

namespace Duck.Physics.Systems
{
    public class SyncPhysicsTransformsSystem : SystemBase
    {
        #region Members

        private IFilter<PhysicsComponent, PhysicsBodyComponent, TransformComponent> _filter;

        private IPhysicsSubsystem _physicsSubsystem;
        private IWorld _world;

        #endregion

        #region Methods

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _world = world;
            _physicsSubsystem = app.GetSubsystem<IPhysicsSubsystem>();

            _filter = Filter<PhysicsComponent, PhysicsBodyComponent, TransformComponent>(world)
                .Build();
        }

        public override void Run()
        {
            var physicsWorld = _physicsSubsystem.GetPhysicsWorld(_world) as PhysicsWorld;

            if (null == physicsWorld) {
                return;
            }

            var simulation = physicsWorld.Simulation;

            foreach (var entity in _filter.EntityList) {
                var physicsCmp = _filter.Get1(entity);
                var bodyCmp =  _filter.Get2(entity);

                ref var transform = ref _filter.Get3(entity);

                if (physicsCmp.BodyType == BodyType.Static) {
                    continue;
                }

                var body = simulation.Bodies.GetBodyReference(bodyCmp.CachedBodyHandle);

                if (!body.Exists) {
                    continue;
                }

                transform.Translation = body.Pose.Position;
                transform.Rotation = body.Pose.Orientation;
            }
        }

        #endregion
    }
}
