using BepuPhysics;
using BepuPhysics.Collidables;
using Duck.Ecs;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class PhysicsCapsuleLifecycleSystem : BasePhysicsShapeLifecycleSystem
{
    #region Members

    private readonly IFilter<PhysicsCapsuleComponent, TransformComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public PhysicsCapsuleLifecycleSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

        _filter = Filter<PhysicsCapsuleComponent, TransformComponent>(world)
            .Without<PhysicsBodyComponent>()
            .Build();
    }

    public override void Run()
    {
        Simulation simulation = _physicsWorld.Simulation;

        foreach (var entityId in _filter.EntityAddedList) {
            IEntity entity = _filter.GetEntity(entityId);
            TransformComponent transformComponent = _filter.Get2(entityId);
            ref PhysicsCapsuleComponent physicsComponent = ref _filter.Get1(entityId);
            ref PhysicsBodyComponent bodyComponent = ref entity.Get<PhysicsBodyComponent>();

            Capsule capsule = new Capsule(physicsComponent.Radius, physicsComponent.Length);
            capsule.ComputeInertia(physicsComponent.Mass);

            RegisterBody(
                simulation, capsule,
                ref bodyComponent,
                physicsComponent.BodyType,
                // bodyInertia,
                new BodyInertia { InverseMass = 1f / physicsComponent.Mass },
                transformComponent.Translation.ToSystem(),
                transformComponent.Rotation.ToSystem(),
                physicsComponent.Radius * 0.02f
            );
        }
    }

    #endregion
}
