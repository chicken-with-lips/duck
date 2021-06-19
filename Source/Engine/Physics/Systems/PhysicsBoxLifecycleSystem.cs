using System.Numerics;
using BepuPhysics.Collidables;
using Duck.Ecs;
using Duck.Physics.Components;

namespace Duck.Physics.Systems
{
    public class PhysicsBoxLifecycleSystem : BasePhysicsShapeLifecycleSystem
    {
        #region Members

        private readonly IFilter<PhysicsBoxComponent, TransformComponent, BoundingBoxComponent> _filter;
        private readonly PhysicsWorld _physicsWorld;

        #endregion

        #region Methods

        public PhysicsBoxLifecycleSystem(IWorld world, IPhysicsSubsystem physicsSubsystem)
        {
            _physicsWorld = (PhysicsWorld) physicsSubsystem.GetOrCreatePhysicsWorld(world);

            _filter = Filter<PhysicsBoxComponent, TransformComponent, BoundingBoxComponent>(world)
                .Without<PhysicsBodyComponent>()
                .Build();
        }

        public override void Run()
        {
            var simulation = _physicsWorld.Simulation;

            foreach (var entityId in _filter.EntityAddedList) {
                IEntity entity = _filter.GetEntity(entityId);
                ref PhysicsBoxComponent physicsComponent = ref _filter.Get1(entityId);
                ref PhysicsBodyComponent bodyComponent = ref entity.Get<PhysicsBodyComponent>();

                TransformComponent transformComponent = _filter.Get2(entityId);
                BoundingBoxComponent boundingBoxComponent = _filter.Get3(entityId);
                Vector3 scale = transformComponent.Scale;

                float width = (Math.Abs(boundingBoxComponent.Box.Min.X) + Math.Abs(boundingBoxComponent.Box.Max.X)) * scale.X;
                float height = (Math.Abs(boundingBoxComponent.Box.Min.Y) + Math.Abs(boundingBoxComponent.Box.Max.Y)) * scale.Y;
                float length = (Math.Abs(boundingBoxComponent.Box.Min.Z) + Math.Abs(boundingBoxComponent.Box.Max.Z)) * scale.Z;

                Box box = new Box(width, height, length);
                box.ComputeInertia(physicsComponent.Mass, out var bodyInertia);

                RegisterBody(
                    simulation,
                    box,
                    ref bodyComponent,
                    physicsComponent.BodyType,
                    bodyInertia,
                    transformComponent.Translation,
                    transformComponent.Rotation
                );
            }
        }

        #endregion
    }
}
