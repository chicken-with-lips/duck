using System;
using BepuPhysics;
using BepuPhysics.Collidables;
using Duck.Contracts.Physics;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.SceneManagement.Components;

namespace Duck.Physics.Systems
{
    public class PhysicsBoxShapeLifecycleSystem : SystemBase
    {
        #region Members

        private readonly IFilter<PhysicsBoxComponent, TransformComponent, BoundingBoxComponent> _filter;
        private readonly IPhysicsSubsystem _physicsSubsystem;
        private readonly IWorld _world;

        #endregion

        #region Methods

        public PhysicsBoxShapeLifecycleSystem(IWorld world, IPhysicsSubsystem physicsSubsystem)
        {
            _world = world;
            _physicsSubsystem = physicsSubsystem;

            _filter = Filter<PhysicsBoxComponent, TransformComponent, BoundingBoxComponent>(world)
                .Without<PhysicsBodyComponent>()
                .Build();
        }

        public override void Run()
        {
            var physicsWorld = _physicsSubsystem.GetPhysicsWorld(_world) as PhysicsWorld;

            if (null == physicsWorld) {
                return;
            }

            var simulation = physicsWorld.Simulation;

            foreach (var entityId in _filter.EntityAddedList) {
                IEntity entity = _filter.GetEntity(entityId);
                ref PhysicsBoxComponent physicsComponent = ref _filter.Get1(entityId);
                ref PhysicsBodyComponent bodyComponent = ref entity.Get<PhysicsBodyComponent>();

                var transformCmp = _filter.Get2(entityId);
                var boundingBoxCmp = _filter.Get3(entityId);
                var scale = transformCmp.Scale;

                var width = (Math.Abs(boundingBoxCmp.Box.Min.X) + Math.Abs(boundingBoxCmp.Box.Max.X)) * scale.X;
                var height = (Math.Abs(boundingBoxCmp.Box.Min.Y) + Math.Abs(boundingBoxCmp.Box.Max.Y)) * scale.Y;
                var length = (Math.Abs(boundingBoxCmp.Box.Min.Z) + Math.Abs(boundingBoxCmp.Box.Max.Z)) * scale.Z;

                var box = new Box(width, height, length);

                if (physicsComponent.BodyType == BodyType.Dynamic) {
                    box.ComputeInertia(physicsComponent.Mass, out var inertia);

                    bodyComponent.CachedShapeIndex = simulation.Shapes.Add(box);
                    bodyComponent.CachedBodyHandle = simulation.Bodies.Add(
                        BodyDescription.CreateDynamic(
                            new RigidPose(
                                transformCmp.Translation,
                                transformCmp.Rotation
                            ),
                            inertia,
                            new CollidableDescription(bodyComponent.CachedShapeIndex, 0.1f),
                            new BodyActivityDescription(0.01f)
                        )
                    );
                } else if (physicsComponent.BodyType == BodyType.Static) {
                    bodyComponent.CachedShapeIndex = simulation.Shapes.Add(box);

                    simulation.Statics.Add(
                        new StaticDescription(
                            transformCmp.Translation,
                            transformCmp.Rotation,
                            new CollidableDescription(bodyComponent.CachedShapeIndex, 0.1f)
                        )
                    );
                }
            }
        }

        #endregion
    }
}
