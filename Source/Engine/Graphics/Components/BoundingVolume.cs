using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Graphics.Components;

public interface IBoundingVolume
{
}

[DuckSerializable]
public partial struct BoundingBoxComponent : IBoundingVolume
{
    public ABox Box = default;

    public BoundingBoxComponent()
    {
    }
}

[DuckSerializable]
public partial struct BoundingSphereComponent : IBoundingVolume
{
    public float Radius = 10f;

    public BoundingSphereComponent()
    {
    }
}
