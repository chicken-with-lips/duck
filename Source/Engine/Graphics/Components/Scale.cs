using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct Scale
{
    public AVector3 Value = AVector3.One;

    public Scale()
    {
    }
}
