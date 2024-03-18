using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.AI.Components;

[AutoSerializable]
public partial struct AgentTargetComponent
{
    public Vector3D<float> Point;
    public Vector3D<float> Heading;
    public Vector3D<float> Velocity;
}
