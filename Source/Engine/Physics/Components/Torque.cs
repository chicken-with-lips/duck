using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct Torque
{
    public AVector3 Value;

    public Torque()
    {
    }
}
