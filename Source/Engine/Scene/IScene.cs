using Duck.Ecs;
using Duck.Scene.Components;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial interface IScene
{
    public string Name { get; }
    public IWorld World { get; }
    public ISystemComposition SystemComposition { get; }
    public int[] Renderables { get; }

    public IScene AddRenderable(int entityId);
    public IScene RemoveRenderable(int entityId);
}
