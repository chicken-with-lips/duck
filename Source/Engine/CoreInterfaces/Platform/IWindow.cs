using Silk.NET.Core.Contexts;
using Silk.NET.Maths;

namespace Duck.Platform;

public interface IWindow
{
    public IntPtr Handle { get; }
    public int Width { get; }
    public int Height { get; }

    bool CloseRequested { get; }
    double ElapsedTime { get; }
    Vector2D<float> CursorPosition { get; }
    public IWindowEvent[] Events { get; }

    void ClearEvents();
    void Update();
}

public struct Configuration
{
    public int Width;
    public int Height;
    public string Title;

    public bool IsResizable;

    public static Configuration Default => new() {
        Width = 1280,
        Height = 1024,
        Title = "Duck",
        IsResizable = true,
    };
}

public interface IWindowEvent
{
}

public struct ResizeEvent : IWindowEvent
{
    public int NewWidth;
    public int NewHeight;
}

public struct KeyEvent : IWindowEvent
{
    public InputName Key;
    public bool IsDown;
}

public struct MousePositionEvent : IWindowEvent
{
    public double X;
    public double Y;
}

public struct MouseButtonEvent : IWindowEvent
{
    public int ButtonIndex;
    public bool IsDown;
}
