using Duck.Graphics.Components;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Physics;

public interface IPhysicsWorld
{
    #region Properties

    Vector3D<float> Gravity { get; set; }

    #endregion

    #region Methods

    public void Step(float timeStep);
    void EmitEvents(IEventBus eventBus);

    bool Overlaps(IBoundingVolume volume, Vector3D<float> position, Quaternion<float> rotation);

    #endregion
}
