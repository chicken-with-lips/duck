using Duck.Serialization;

namespace Duck.Graphics.Components;

[DuckSerializable]
public partial struct ParentComponent
{
    public int ParentEntityId = default;

    public ParentComponent()
    {
    }
}
