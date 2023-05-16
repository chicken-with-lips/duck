using Duck.Serialization;

namespace Duck.Renderer.Components;

[AutoSerializable]
public partial struct ParentComponent
{
    public int ParentEntityId = default;

    public ParentComponent()
    {
    }
}
