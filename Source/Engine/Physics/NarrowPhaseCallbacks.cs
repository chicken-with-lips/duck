using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using Duck.Ecs;
using Duck.Physics.CharacterController;

namespace Duck.Physics
{
    internal struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        private readonly IWorld _world;
        private readonly CharacterControllerIntegration _characterControllerIntegration;
        private readonly SpringSettings _contactSpringiness;

        public NarrowPhaseCallbacks(CharacterControllerIntegration characterControllerIntegration, IWorld world)
        {
            _characterControllerIntegration = characterControllerIntegration;
            _world = world;
            _contactSpringiness = new SpringSettings(30, 1);
        }

        public void Initialize(Simulation simulation)
        {
            _characterControllerIntegration.Initialize(simulation, _world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties() {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = 2f,
                SpringSettings = _contactSpringiness
            };

            _characterControllerIntegration.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
