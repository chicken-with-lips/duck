using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct Impulse
{
    public AVector3 Value;

    public Impulse()
    {
    }
}
