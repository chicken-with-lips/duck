using Arch.Core;
using Duck.Serialization;

namespace Duck.AI.Components;

[AutoSerializable]
public partial struct AgentTargetEntityComponent
{
    public EntityReference? Value;
}
