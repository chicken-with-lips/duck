using System.Numerics;
using System.Runtime.InteropServices;
using ChickenWithLips.PhysX.Net;
using Duck.Ecs;
using Duck.Physics.Events;
using Duck.ServiceBus;
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
    private readonly Dictionary<PxActor, IEntity> _actorToEntityMap = new();
    private readonly SimulationEventCallback _simulationEventCallback;
    private readonly SimulationFilterCallback _simulationFilterCallback;

    #endregion

    #region Methods

    public PhysicsWorld(IWorld world, PxPhysics physics, PxCpuDispatcher cpuDispatcher)
    {
        _physics = physics;
        _simulationEventCallback = new SimulationEventCallback();
        _simulationFilterCallback = new SimulationFilterCallback();

        var sceneDesc = new PxSceneDesc(_physics.TolerancesScale) {
            CpuDispatcher = cpuDispatcher,
            Gravity = new Vector3(0, -9.8f, 0),
            SimulationEventCallback = _simulationEventCallback,
            FilterCallback = _simulationFilterCallback,
            FilterShader = FilterShader
        };

        _scene = _physics.CreateScene(sceneDesc);
    }

    private PxFilterFlag FilterShader(PxFilterObjectFlag attributes0, ref PxFilterData filterData0, PxFilterObjectFlag attributes1, ref PxFilterData filterData1, out PxPairFlag pairFlags)
    {
        if (PxFilterObject.IsTrigger(attributes0) || PxFilterObject.IsTrigger(attributes1)) {
            pairFlags = PxPairFlag.TriggerDefault;
            return PxFilterFlag.Default;
        }

        pairFlags = PxPairFlag.ContactDefault;

        // if ((filterData0.Word0 & filterData1.Word1) != 0 && (filterData1.Word0 & filterData0.Word1) != 0) {
        pairFlags |= PxPairFlag.NotifyTouchFound;
        // }

        return PxFilterFlag.Default;
    }

    public void EmitEvents(IEventBus eventBus)
    {
        foreach (var pairHeader in _simulationEventCallback.Contacts) {
            eventBus.Enqueue(
                new PhysicsCollision(
                    GetEntityForActor(pairHeader.Actors[0]),
                    GetEntityForActor(pairHeader.Actors[1])
                )
            );
        }
    }

    public void Step(float timeStep)
    {
        _simulationEventCallback.ClearContacts();

        _scene.Simulate(timeStep);
        _scene.FetchResults(true);
    }
    
    internal void MapActorToEntity(IEntity entity, PxActor actor)
    {
        _actorToEntityMap.Add(actor, entity);
    }

    internal IEntity GetEntityForActor(PxActor actor)
    {
        return _actorToEntityMap[actor];
    }

    #endregion


    internal class SimulationEventCallback : PxSimulationEventCallback
    {
        public List<PxContactPairHeader> Contacts { get; } = new();

        public void ClearContacts()
        {
            Contacts.Clear();
        }

        protected override void OnContact(PxContactPairHeader pairHeader, PxContactPair[] pairs)
        {
            Contacts.Add(pairHeader);
        }
    }

    internal class SimulationFilterCallback : PxSimulationFilterCallback
    {
        protected override bool OnStatusChange(ref uint pairId, ref PxPairFlag pairFlags, ref PxFilterFlag filterFlags)
        {
            return false;
        }
    }
}
