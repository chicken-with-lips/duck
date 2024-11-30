using Duck.Serialization;

namespace Duck.Physics.Components;

[DuckSerializable]
public partial struct Torque
{
    public AVector3 Value;
}
