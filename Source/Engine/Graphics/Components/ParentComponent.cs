using Duck.Serialization;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct ParentComponent
{
    public int ParentEntityId = default;

    public ParentComponent()
    {
    }
}
