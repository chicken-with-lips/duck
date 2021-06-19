using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Duck.Ecs;
using Duck.Physics.CharacterController;

namespace Duck.Physics
{
    public class PhysicsWorld : IPhysicsWorld
    {
        #region Properties

        public Simulation Simulation => _simulation;
        public CharacterControllerIntegration CharacterControllerIntegration => _characterControllerIntegration;

        #endregion

        #region Members

        private readonly IWorld _world;
        private readonly IThreadDispatcher _threadDispatcher;
        private readonly Simulation _simulation;
        private readonly CharacterControllerIntegration _characterControllerIntegration;

        #endregion

        #region Methods

        public PhysicsWorld(IWorld world, BufferPool bufferPool, IThreadDispatcher threadDispatcher)
        {
            _characterControllerIntegration = new CharacterControllerIntegration();

            _world = world;
            _threadDispatcher = threadDispatcher;
            _simulation = Simulation.Create(
                bufferPool,
                new NarrowPhaseCallbacks(_characterControllerIntegration, _world),
                new PoseIntegratorCallbacks(new Vector3(0, -10f, 0)),
                // new SubsteppingTimestepper(4)
                new PositionFirstTimestepper()
            );
        }

        public void Step(float timeStep)
        {
            _simulation.Timestep(timeStep, _threadDispatcher);
        }

        #endregion
    }
}
