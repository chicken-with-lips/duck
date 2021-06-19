using System.Numerics;

namespace Duck.Platform;

public interface IWindow
{
    bool CloseRequested { get; }
    double ElapsedTime { get; }
    Vector2 CursorPosition { get; }
    public IWindowEvent[] Events { get; }

    void Update();
    void ClearEvents();
}

public struct Configuration
{
    public int Width;
    public int Height;
    public string Title;

    public bool IsResizable;

    public static Configuration Default => new() {
        Width = 1024,
        Height = 768,
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
