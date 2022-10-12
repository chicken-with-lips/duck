using Duck.Ecs;
using Duck.Scene.Components;
using Duck.Scene.Scripting;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial interface IScene
{
    public bool IsActive { get; set; }
    public ISceneScript? Script { get; set; }

    public string Name { get; }
    public IWorld World { get; }
    public ISystemComposition SystemComposition { get; }
    public int[] Renderables { get; }

    public void Tick();

    public IScene AddRenderable(int entityId);
    public IScene RemoveRenderable(int entityId);
}
