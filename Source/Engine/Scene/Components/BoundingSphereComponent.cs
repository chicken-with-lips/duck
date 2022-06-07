using Duck.Serialization;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct BoundingSphereComponent
{
    public float Radius = 10f;
}
