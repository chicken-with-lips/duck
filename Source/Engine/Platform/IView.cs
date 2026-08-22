using Arch.Core;
using Duck.Scene;
using Silk.NET.Maths;

namespace Duck.Platform;

public interface IView
{
    string Name { get; }
    bool IsEnabled { get; set; }
    Vector2D<int> Dimensions { get; set; }
    bool AutoSizeToWindow { get; set; }
    Vector2D<int> Position { get; set; }
    bool IsPrimary { get; }
    IScene? Scene { get; set; }

    Entity Camera { get; set; }
}