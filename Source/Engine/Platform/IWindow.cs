using Silk.NET.Maths;

namespace Duck.Platform;

public interface IWindow
{
    public Vector2D<int> Dimensions { get; }
    public bool IsPrimary { get; }
}

public struct WindowConfiguration
{
    public Vector2D<int> Dimensions;
    public string Title;

    public bool IsResizable;

    public static WindowConfiguration Default => new() {
        Dimensions = new Vector2D<int>(1280, 1024),
        Title = "Duck",
        IsResizable = true,
    };
}