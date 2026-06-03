using Duck.Platform;
using Duck.RenderSystem.Godot.Extensions;
using Godot;
using Silk.NET.Maths;

namespace Duck.RenderSystem.Godot;

public class GodotWindow : IWindow
{
    public Vector2D<int> Dimensions => DisplayServer.Singleton.WindowGetSize(_windowId).ToVector2D();

    public bool IsPrimary { get; }

    private readonly int _windowId;

    internal GodotWindow(int windowId, bool isPrimary)
    {
        _windowId = windowId;
        IsPrimary = isPrimary;
    }
}