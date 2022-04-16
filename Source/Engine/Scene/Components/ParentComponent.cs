using Duck.Serialization;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct ParentComponent
{
    public int ParentEntityId = default;
}
