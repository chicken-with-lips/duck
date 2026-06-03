using Duck.Platform;
using Duck.Scene;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace Duck.RenderSystem.Vulkan;

public class VulkanWindow : IWindow
{
    public Vector2D<int> Dimensions
    {
        get => new(_silkWindow.Size.X, _silkWindow.Size.Y);
    }

    public bool IsPrimary { get; }

    public IReadOnlyList<VulkanView> Views
    {
        get => _views;
    }

    internal Silk.NET.Windowing.IWindow SilkWindow
    {
        get => _silkWindow;
    }

    internal SurfaceKHR Surface { get; private set; }
    internal SwapchainKHR Swapchain { get; private set; }
    internal Format SwapchainFormat { get; private set; }
    internal Extent2D SwapchainExtent { get; private set; }
    internal Image[] SwapchainImages { get; private set; } = [];
    internal ImageView[] SwapchainImageViews { get; private set; } = [];

    private readonly Silk.NET.Windowing.IWindow _silkWindow;
    private readonly List<VulkanView> _views = [];

    internal VulkanWindow(Silk.NET.Windowing.IWindow silkWindow, bool isPrimary)
    {
        _silkWindow = silkWindow;
        IsPrimary = isPrimary;
    }

    public VulkanView CreateView(string name, bool isPrimary = false)
    {
        var view = new VulkanView(name, isPrimary, Dimensions);
        _views.Add(view);
        return view;
    }

    internal void SetSurface(SurfaceKHR surface)
    {
        Surface = surface;
    }

    internal void SetSwapchain(SwapchainKHR swapchain,
        Format format,
        Extent2D extent,
        Image[] images,
        ImageView[] imageViews)
    {
        Swapchain = swapchain;
        SwapchainFormat = format;
        SwapchainExtent = extent;
        SwapchainImages = images;
        SwapchainImageViews = imageViews;
    }
}
