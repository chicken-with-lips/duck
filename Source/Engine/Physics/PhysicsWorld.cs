using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using ChickenWithLips.PhysX;
using Duck.Physics.Events;
using Duck.Renderer.Components;
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

    private readonly World _world;
    private readonly PxPhysics _physics;
    private readonly PxScene _scene;
    private readonly Dictionary<PxActor, EntityReference> _actorToEntityMap = new();
    private readonly Dictionary<EntityReference, PxActor> _entityToActorMap = new();
    private readonly SimulationEventCallback _simulationEventCallback;
    private readonly SimulationFilterCallback _simulationFilterCallback;
    private readonly PxFilterShaderCallback _filterShader;

    #endregion

    #region Methods

    public PhysicsWorld(World world, PxPhysics physics, PxCpuDispatcher cpuDispatcher)
    {
        _world = world;
        _physics = physics;
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

        _simulationEventCallback.ClearContacts();
    }

    public bool Overlaps(IBoundingVolume volume, Vector3D<float> position, Quaternion<float> rotation)
    {
        var transform = new PxTransform(rotation.ToSystem(), position.ToSystem());
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

        throw new ApplicationException("TODO: errors");
    }

    public void Step(float timeStep)
    {
        _scene.Simulate(timeStep);
        _scene.FetchResults(true);
    }

    public PxRigidBody? GetRigidBody(Entity entity)
    {
        if (_entityToActorMap.TryGetValue(entity.Reference(), out var actor)) {
            return actor as PxRigidBody;
        }

        return null;
    }

    internal void MapActorToEntity(Entity entity, PxActor actor)
    {
        _actorToEntityMap.Add(actor, entity.Reference());
        _entityToActorMap.Add(entity.Reference(), actor);
    }

    internal void UnmapActor(PxActor actor)
    {
        _entityToActorMap.Remove(_actorToEntityMap[actor]);
        _actorToEntityMap.Remove(actor);
    }

    internal EntityReference GetEntityForActor(PxActor actor)
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
