using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Duck.Ecs.Systems;
using Duck.Physics.Components;

namespace Duck.Physics.Systems;

public abstract class BasePhysicsShapeLifecycleSystem : SystemBase
{
    protected void RegisterBody<TShape>(Simulation simulation, TShape shape, ref PhysicsBodyComponent bodyComponent, BodyType bodyType, BodyInertia bodyInertia, Vector3 position, Quaternion rotation, float sleepThreshold = 0.01f) where TShape : unmanaged, IShape
    {
        bodyComponent.ShapeIndex = simulation.Shapes.Add(shape);

        if (bodyType == BodyType.Dynamic) {
            bodyComponent.BodyHandle = simulation.Bodies.Add(
                BodyDescription.CreateDynamic(
                    new RigidPose(position, rotation),
                    bodyInertia,
                    new CollidableDescription(bodyComponent.ShapeIndex, 0.1f),
                    new BodyActivityDescription(sleepThreshold)
                )
            );
            bodyComponent.IsDynamic = true;
        } else if (bodyType == BodyType.Kinematic) {
            bodyComponent.BodyHandle = simulation.Bodies.Add(
                BodyDescription.CreateKinematic(
                    position,
                    // bodyInertia,
                    new BodyVelocity(),
                    new CollidableDescription(bodyComponent.ShapeIndex, 0.1f),
                    new BodyActivityDescription(sleepThreshold)));

            bodyComponent.IsDynamic = true;
        } else if (bodyType == BodyType.Static) {
            simulation.Statics.Add(
                new StaticDescription(
                    position,
                    rotation,
                    bodyComponent.ShapeIndex
                )
            );
        }
    }
}
