using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct AngularDamping
{
    public float Value;

    public AngularDamping()
    {
    }
}
