using Duck.Serialization;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct LocalToWorld
{
    public AMatrix4X4 Value;

    public LocalToWorld()
    {
    }
}
