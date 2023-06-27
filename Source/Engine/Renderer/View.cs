using Arch.Core;
using Arch.Core.Extensions;
using Duck.Renderer.Components;
using Silk.NET.Maths;

namespace Duck.Renderer;

public class View
{
    public string Name { get; }
    public bool IsEnabled { get; set; }
    public Vector2D<int> Dimensions { get; set; }
    public bool AutoSizeToWindow { get; set; } = true;
    public Vector2D<int> Position { get; set; }

    public WeakReference<IScene>? Scene { get; set; }
    public EntityReference? Camera { get; set; }

    public bool IsValid {
        get {
            if (!IsEnabled) {
                return false;
            }

            if (null == Scene || !Scene.TryGetTarget(out var scene) || !scene.IsActive) {
                return false;
            }

            if (!Camera.HasValue || !Camera.Value.IsAlive() || !Camera.Value.Entity.Has<CameraComponent, TransformComponent>()) {
                return false;
            }

            return true;
        }
    }

    public View(string name)
    {
        Name = name;
    }
}
