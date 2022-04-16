using BepuPhysics;
using BepuPhysics.Collidables;
using Duck.Ecs;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class PhysicsBoxLifecycleSystem : BasePhysicsShapeLifecycleSystem
{
    #region Members

    private readonly IFilter<PhysicsBoxComponent, TransformComponent, BoundingBoxComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public PhysicsBoxLifecycleSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

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
            Vector3D<float> scale = transformComponent.Scale;

            float width = (System.Math.Abs(boundingBoxComponent.Box.Min.X) + System.Math.Abs(boundingBoxComponent.Box.Max.X)) * scale.X;
            float height = (System.Math.Abs(boundingBoxComponent.Box.Min.Y) + System.Math.Abs(boundingBoxComponent.Box.Max.Y)) * scale.Y;
            float length = (System.Math.Abs(boundingBoxComponent.Box.Min.Z) + System.Math.Abs(boundingBoxComponent.Box.Max.Z)) * scale.Z;

            Box box = new Box(width, height, length);
            BodyInertia bodyInertia = box.ComputeInertia(physicsComponent.Mass);

            RegisterBody(
                simulation,
                box,
                ref bodyComponent,
                physicsComponent.BodyType,
                bodyInertia,
                transformComponent.Translation.ToSystem(),
                transformComponent.Rotation.ToSystem()
            );
        }
    }

    #endregion
}
