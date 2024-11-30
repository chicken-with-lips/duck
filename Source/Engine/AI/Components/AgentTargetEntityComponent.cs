using Arch.Core;
using Duck.Serialization;

namespace Duck.AI.Components;

[DuckSerializable]
public partial struct AgentTargetEntityComponent
{
    public EntityReference Value;
}
