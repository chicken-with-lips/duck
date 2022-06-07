using Duck.Ecs;
using Duck.Scene.Components;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial interface IScene
{
    public IWorld World { get; }
    public int[] Renderables { get; }

    public IScene AddRenderable(int entityId);
    public IScene RemoveRenderable(int entityId);
}
