using Duck.Scene;
using Godot;

namespace Duck.RenderSystem.Godot;

public class GodotSceneLink
{
    public IScene Scene { get; }
    public Node3D GodotScene { get; }

    public GodotSceneLink(IScene scene, Node3D godotScene)
    {
        Scene = scene;
        GodotScene = godotScene;
    }
}