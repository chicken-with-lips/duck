using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial interface IScene
{
    public IWorld World { get; }
}
