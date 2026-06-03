using Arch.Core;
using Silk.NET.Maths;

namespace Duck.Scene;

public interface IView
{
    public string Name { get; }
    public bool IsEnabled { get; set; }
    public Vector2D<int> Dimensions { get; set; }
    public bool AutoSizeToWindow { get; set; }
    public Vector2D<int> Position { get; set; }
    bool IsPrimary { get; }
    public IScene? Scene { get; set; }

    public Entity Camera { get; set; }
}
