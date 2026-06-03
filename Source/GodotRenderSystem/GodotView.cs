using Arch.Core;
using Arch.Core.Extensions;
using Duck.Scene;
using Duck.RenderSystem.Godot.Components;
using Godot;
using Silk.NET.Maths;

namespace Duck.RenderSystem.Godot;

public class GodotView : IView
{
    public string Name { get; }

    public bool IsPrimary { get; }

    public bool IsEnabled {
        get;
        set {
            field = value;
            _isEnabledDirty = true;
        }
    }

    public Vector2D<int> Dimensions {
        get;
        set {
            field = value;
            _isDimensionsDirty = true;
        }
    }


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

                _isCameraDirty = true;
            }
        }
    }

    public Entity Camera {
        get;
        set {
            field = value;

            _isCameraDirty = true;
        }
    }

    public bool AutoSizeToWindow { get; set; }
    public Vector2D<int> Position { get; set; }

    private readonly GodotRenderModule _module;
    private readonly Rid _viewportRid;
    private WeakReference<IScene>? _scene;

    private bool _isEnabledDirty;
    private bool _isDimensionsDirty;
    private bool _isCameraDirty;

    internal GodotView(GodotRenderModule module, in Rid viewportRid, in string name, in bool isPrimary,
        in Vector2D<int> dimensions)
    {
        _module = module;
        _viewportRid = viewportRid;

        Name = name;
        IsPrimary = isPrimary;
        IsEnabled = true;
        Dimensions = dimensions;

        _isEnabledDirty = false;
        _isCameraDirty = false;
        _isDimensionsDirty = false;
    }

    public void Tick()
    {
        if (_isEnabledDirty || _isDimensionsDirty) {
            RenderingServer.CallOnRenderThread(Callable.From(() => {

                if (_isEnabledDirty) {
                    RenderingServer.ViewportSetActive(_viewportRid, IsEnabled);
                }

                if (_isDimensionsDirty) {
                    RenderingServer.ViewportSetSize(_viewportRid, Dimensions.X, Dimensions.Y);
                }
            }));

            _isEnabledDirty = false;
            _isDimensionsDirty = false;
        }

        if (_isCameraDirty) {
            if (Scene == null) {
                _isCameraDirty = false;
            } else if (Camera.Has<GodotCameraHandle>()) {
                RenderingServer.CallOnRenderThread(Callable.From(() => {
                    RenderingServer.ViewportAttachCamera(_viewportRid, Camera.Get<GodotCameraHandle>().Value);
                }));

                _isCameraDirty = false;
            }
        }
    }
}