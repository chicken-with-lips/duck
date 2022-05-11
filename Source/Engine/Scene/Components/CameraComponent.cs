using Duck.Serialization;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct CameraComponent
{
    public bool IsActive = default;
}
