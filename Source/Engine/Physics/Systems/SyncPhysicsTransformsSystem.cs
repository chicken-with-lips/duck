using BepuPhysics;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Silk.NET.Maths;

namespace Duck.Physics.Systems;

public class SyncPhysicsTransformsSystem : SystemBase
{
    #region Members

    private readonly IFilter<PhysicsBodyComponent, TransformComponent> _filter;
    private readonly PhysicsWorld _physicsWorld;

    #endregion

    #region Methods

    public SyncPhysicsTransformsSystem(IWorld world, IPhysicsModule physicsModule)
    {
        _physicsWorld = (PhysicsWorld)physicsModule.GetOrCreatePhysicsWorld(world);

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
                body.Pose.Position = transform.Translation.ToSystem();
            } else {
                transform.Translation = body.Pose.Position.ToGeneric();
            }

            if (transform.IsRotationDirty) {
                body.Pose.Orientation = transform.Rotation.ToSystem();
            } else {
                transform.Rotation = body.Pose.Orientation.ToGeneric();
            }

            transform.ClearDirtyFlags();
        }
    }

    #endregion
}
