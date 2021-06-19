using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Duck.Contracts.Physics;

namespace Duck.Physics
{
    public class PhysicsWorld : IPhysicsWorld
    {
        #region Properties

        public Simulation Simulation => _simulation;

        #endregion

        #region Members

        private readonly IThreadDispatcher _threadDispatcher;
        private readonly Simulation _simulation;

        #endregion

        #region Methods

        public PhysicsWorld(BufferPool bufferPool, IThreadDispatcher threadDispatcher)
        {
            _threadDispatcher = threadDispatcher;
            _simulation = Simulation.Create(
                bufferPool,
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new Vector3(0, -10f, 0)),
                new PositionLastTimestepper()
            );
        }

        public void Step(float timeStep)
        {
            _simulation.Timestep(timeStep, _threadDispatcher);
        }

        #endregion
    }
}
