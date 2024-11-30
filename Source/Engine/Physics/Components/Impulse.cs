using Duck.Serialization;

namespace Duck.Physics.Components;

[DuckSerializable]
public partial struct Impulse
{
    public AVector3 Value;
}
