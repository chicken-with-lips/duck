using Arch.Core;
using Duck.Platform;
using Duck.Scene;
using Silk.NET.Maths;

namespace Duck.RenderSystem.Vulkan;

public class VulkanView : IView
{
    public string Name { get; }
    public bool IsPrimary { get; }

    public bool IsEnabled { get; set; } = true;
    public bool AutoSizeToWindow { get; set; } = true;
    public Vector2D<int> Dimensions { get; set; }
    public Vector2D<int> Position { get; set; }

    public IScene? Scene { get; set; }
    public Entity Camera { get; set; }

    internal VulkanView(string name, bool isPrimary, Vector2D<int> dimensions)
    {
        Name = name;
        IsPrimary = isPrimary;
        Dimensions = dimensions;
    }
}
