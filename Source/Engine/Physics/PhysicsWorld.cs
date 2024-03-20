using Arch.Core;
using Duck.Graphics.Components;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Physics;

public class PhysicsWorld : IPhysicsWorld
{
    #region Properties

    public Vector3D<float> Gravity
    {
        get
        {
            Console.WriteLine("TODO: PhysicsWorld.GetGravity");
            return Vector3D<float>.Zero;
        }

        set
        {
            Console.WriteLine("TODO: PhysicsWorld.SetGravity");
        }
    }

    #endregion

    #region Members

    private readonly World _world;

    #endregion

    #region Methods

    public PhysicsWorld(World world)
    {
        _world = world;


        Console.WriteLine("TODO: PhysicsWorld.Ctor");
        /*_physics = physics;
        _simulationEventCallback = new SimulationEventCallback();
        _simulationFilterCallback = new SimulationFilterCallback();
        _filterShader = FilterShader;

        var sceneDesc = new PxSceneDesc(_physics.TolerancesScale) {
            CpuDispatcher = cpuDispatcher,
            Gravity = new Vector3(0, -9.8f, 0),
            SimulationEventCallback = _simulationEventCallback,
            FilterCallback = _simulationFilterCallback,
            FilterShader = _filterShader
        };

        _scene = _physics.CreateScene(sceneDesc);*/
    }

    public void EmitEvents(IEventBus eventBus)
    {
        /*foreach (var pairHeader in _simulationEventCallback.Contacts) {
            _world.Create(
                new PhysicsCollision {
                    A = GetEntityForActor(pairHeader.Actors[0]),
                    B = GetEntityForActor(pairHeader.Actors[1]),
                });

            _world.Create(
                new PhysicsCollision {
                    A = GetEntityForActor(pairHeader.Actors[1]),
                    B = GetEntityForActor(pairHeader.Actors[0]),
                });
        }

        _simulationEventCallback.ClearContacts();*/
        Console.WriteLine("TODO: PhysicsWorld.EmitEvents");
    }

    public bool Overlaps(IBoundingVolume volume, Vector3D<float> position, Quaternion<float> rotation)
    {
        /*var transform = new PxTransform(rotation.ToSystem(), position.ToSystem());
        var flags = PxQueryFlag.AnyHit | PxQueryFlag.Dynamic | PxQueryFlag.Static;

        if (volume is BoundingBoxComponent boxVolume) {
            return _scene.Overlap(
                PhysXHelper.CreateBoxGeometry(boxVolume.Box, Vector3D<float>.One),
                transform,
                flags
            );
        } else if (volume is BoundingSphereComponent sphereVolume) {
            return _scene.Overlap(
                PhysXHelper.CreateSphereGeometry(sphereVolume, Vector3D<float>.One),
                transform,
                flags
            );
        }

        throw new ApplicationException("TODO: errors");*/

        Console.WriteLine("TODO: PhysicsWorld.Overlaps");

        return false;
    }

    public void Step(float timeStep)
    {
        Console.WriteLine("TODO: PhysicsWorld.Step");
        // _scene.Simulate(timeStep);
        // _scene.FetchResults(true);
    }

    #endregion
}
