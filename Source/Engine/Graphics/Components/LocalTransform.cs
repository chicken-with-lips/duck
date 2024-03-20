using Duck.Serialization;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct LocalTransform
{
    public AVector3 Position;
    public AQuaternion Orientation;
    public AVector3 Scale;

    public AVector3 Up;
    public AVector3 Forward;
    public AVector3 Right;

    public LocalTransform()
    {
    }
}
