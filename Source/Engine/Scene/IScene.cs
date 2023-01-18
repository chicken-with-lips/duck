using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial interface IScene
{
    public bool IsActive { get; set; }

    public string Name { get; }
    public IWorld World { get; }
    public int[] Renderables { get; }

    public void Tick();

    public IScene AddRenderable(int entityId);
    public IScene RemoveRenderable(int entityId);
}
