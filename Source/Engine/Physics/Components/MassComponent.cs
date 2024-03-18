using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct MassComponent
{
    public float Value = 1;
    public float ForceMultiplier = 1;
}
