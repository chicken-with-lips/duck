using Arch.Core;
using Silk.NET.Maths;

namespace Duck.Renderer;

public class View
{
    public string Name { get; }
    public bool IsEnabled { get; set; }
    public Vector2D<int> Dimensions { get; set; }
    public Vector2D<int> Position { get; set; }

    public WeakReference<IScene> Scene { get; set; }
    public EntityReference Camera { get; set; }

    public View(string name)
    {
        Name = name;
    }
}
