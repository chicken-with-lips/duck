using Arch.Core;
using Arch.Core.Extensions;
using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Renderer;

public class View
{
    public string Name { get; }
    public bool IsEnabled { get; set; } = true;
    public Vector2D<int> Dimensions { get; set; }
    public bool AutoSizeToWindow { get; set; } = true;
    public Vector2D<int> Position { get; set; }

    public IScene? Scene {
        get {
            if (_scene == null) {
                return null;
            }

            _scene.TryGetTarget(out var scene);

            return scene;
        }
        set {
            if (value == null) {
                _scene = null;
            } else {
                _scene = new WeakReference<IScene>(value);
            }
        }
    }

    private WeakReference<IScene>? _scene;

    public EntityReference? Camera { get; set; }

    public bool IsValid {
        get {
            if (!IsEnabled) {
                return false;
            }

            if (null == Scene || !Scene.IsActive) {
                return false;
            }

            if (!Camera.HasValue || !Scene.World.IsAlive(Camera.Value) || !Scene.World.Has<CameraComponent, TransformComponent>(Camera.Value.Entity)) {
                return false;
            }

            return true;
        }
    }

    public View(string name)
    {
        Name = name;
    }

    public void ClearSceneReference()
    {
        Scene = null;
        Camera = null;
    }
}
