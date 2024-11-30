using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.AI.Components;

[DuckSerializable]
public partial struct AgentComponent
{
    public Vector3D<float> SteeringForce;
}
