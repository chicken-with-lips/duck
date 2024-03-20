using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct LinearDamping
{
    public float Value;

    public LinearDamping()
    {
    }
}
