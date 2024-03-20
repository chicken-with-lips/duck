using Arch.Core;
using Duck.Graphics.Components;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Physics;

public interface IPhysicsScene : IDisposable
{
    #region Properties

    public Vector3D<float> Gravity { get; set; }
    public bool IsPaused { get; set; }

    #endregion

    #region Methods

    public void Step(float timeStep);
    public void EmitEvents(IEventBus eventBus);

    public bool Overlaps(IBoundingVolume volume, Vector3D<float> position, Quaternion<float> rotation);

    #endregion
}
