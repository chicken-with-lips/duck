using System.Numerics;
using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Silk.NET.Maths;

namespace Duck.Physics;

public class PhysicsWorld : IPhysicsWorld
{
    #region Properties

    internal PxPhysics Physics => _physics;
    internal PxScene Scene => _scene;

    public Vector3D<float> Gravity {
        get => _scene.Gravity.ToGeneric();
        set => _scene.Gravity = value.ToSystem();
    }

    #endregion

    #region Members

    private readonly PxPhysics _physics;
    private readonly PxScene _scene;

    #endregion

    #region Methods

    public PhysicsWorld(IWorld world, PxPhysics physics, PxCpuDispatcher cpuDispatcher)
    {
        _physics = physics;

        var sceneDesc = new PxSceneDesc(_physics.TolerancesScale) {
            CpuDispatcher = cpuDispatcher,
            Gravity = new Vector3(0, -9.8f, 0),
        };

        _scene = _physics.CreateScene(sceneDesc);
    }

    public void Step(float timeStep)
    {
        _scene.Simulate(timeStep);
        _scene.FetchResults(true);
    }

    #endregion
}
