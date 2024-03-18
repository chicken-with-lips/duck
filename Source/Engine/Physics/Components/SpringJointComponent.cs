using Arch.Core;
using Duck.Serialization;

namespace Duck.Physics.Components;

[AutoSerializable]
public partial struct SpringJointComponent
{
    public bool IsTargetDirty { get; private set; }

    private EntityReference _target;

    public EntityReference Target {
        get => _target;
        set {
            _target = value;
            IsTargetDirty = true;
        }
    }

    public SpringJointComponent()
    {
    }

    public void ClearDirtyFlags()
    {
        IsTargetDirty = false;
    }
}
