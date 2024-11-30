using Arch.Core;
using Duck.Serialization;

namespace Duck.Physics.Components;

[DuckSerializable]
public partial struct SpringJointComponent
{
    public bool IsTargetDirty { get; private set; }

    public EntityReference Target {
        get => _target;
        set {
            _target = value;
            IsTargetDirty = true;
        }
    }
    private EntityReference _target;

    public void ClearDirtyFlags()
    {
        IsTargetDirty = false;
    }
}
