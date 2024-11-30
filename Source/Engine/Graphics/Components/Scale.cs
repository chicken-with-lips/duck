using Duck.Serialization;

namespace Duck.Graphics.Components;

[DuckSerializable]
public partial struct Scale
{
    public AVector3 Value = AVector3.One;

    public Scale()
    {
    }
}
