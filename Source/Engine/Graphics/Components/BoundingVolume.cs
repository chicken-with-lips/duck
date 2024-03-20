using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Graphics.Components;

public interface IBoundingVolume
{
}

[AutoSerializable]
public partial struct BoundingBoxComponent : IBoundingVolume
{
    public ABox Box = default;

    public BoundingBoxComponent()
    {
    }
}

[AutoSerializable]
public partial struct BoundingSphereComponent : IBoundingVolume
{
    public float Radius = 10f;

    public BoundingSphereComponent()
    {
    }
}
