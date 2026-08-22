using System.Runtime.InteropServices;
using System.Text;
using Duck.Platform;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;
using DuckIView = Duck.Platform.IView;
using DuckIWindow = Duck.Platform.IWindow;
using Logger = Duck.Platform.Logging.Logger;

namespace Duck.RenderSystem.Vulkan;

public unsafe class VulkanPlatform : IPlatform
{
    public DuckIWindow PrimaryWindow {
        get => _windows.First(w => w.IsPrimary);
    }

    public DuckIView PrimaryView {
        get => _views.First(v => v.IsPrimary);
    }

    public IReadOnlyList<VulkanWindow> Windows {
        get => _windows;
    }

    public Vk Vk {
        get => _vk!;
    }

    public Instance Instance {
        get => _instance;
    }

    public PhysicalDevice PhysicalDevice {
        get => _physicalDevice;
    }

    public Device Device {
        get => _device;
    }

    public Queue GraphicsQueue {
        get => _graphicsQueue;
    }

    public Queue PresentQueue {
        get => _presentQueue;
    }

    public uint GraphicsQueueFamily {
        get => _graphicsQueueFamily;
    }

    public uint PresentQueueFamily {
        get => _presentQueueFamily;
    }

    public KhrSurface KhrSurface {
        get => _khrSurface!;
    }

    public KhrSwapchain KhrSwapchain {
        get => _khrSwapchain!;
    }

    public event Action<VulkanWindow>? WindowCreated;

    private Vk? _vk;
    private Instance _instance;
    private KhrSurface? _khrSurface;
    private KhrSwapchain? _khrSwapchain;

#if DEBUG
    private ExtDebugUtils? _extDebugUtils;
    private DebugUtilsMessengerEXT _debugMessenger;
    private PfnDebugUtilsMessengerCallbackEXT _debugCallback;
#endif

    private PhysicalDevice _physicalDevice;
    private Device _device;
    private Queue _graphicsQueue;
    private Queue _presentQueue;
    private uint _graphicsQueueFamily;
    private uint _presentQueueFamily;

    private readonly List<VulkanWindow> _windows = [];
    private readonly List<VulkanView> _views = [];
    private readonly Logger _logger;
    private bool _windowingRegistered;

    private HashSet<string>? _supportedDeviceExtensions;
    private HashSet<string>? _supportedInstanceExtensions;

    public VulkanPlatform(Logger logger)
    {
        _logger = logger;
    }

    public bool Initialize(IApplication app)
    {
        _logger.LogInformation("Initializing Vulkan platform");

        var primaryWindow = CreateWindowInternal(WindowConfiguration.Default, true);

        _vk = Vk.GetApi();

        if (!CreateInstance(primaryWindow)) {
            return false;
        }

#if DEBUG
        SetupDebugMessenger();
#endif

        if (!_vk.TryGetInstanceExtension<KhrSurface>(_instance, out _khrSurface)) {
            throw new InvalidOperationException("KHR_surface extension not available");
        }

        if (!CreateSurface(primaryWindow)) {
            return false;
        }

        if (!SelectPhysicalDevice(primaryWindow.Surface)) {
            return false;
        }

        if (!CreateLogicalDevice()) {
            return false;
        }

        if (!_vk.TryGetDeviceExtension<KhrSwapchain>(_instance, _device, out _khrSwapchain)) {
            _logger.LogError("KHR_swapchain extension not available");
            return false;
        }

        if (!CreateSwapchain(primaryWindow)) {
            return false;
        }

        var primaryView = primaryWindow.CreateView("Primary", true);
        _views.Add(primaryView);

        _logger.LogInformation("Vulkan platform initialized");

        return true;
    }

    public void Shutdown()
    {
        _vk!.DeviceWaitIdle(_device);

        foreach (var window in _windows) {
            DestroyWindowResources(window);
        }

        _vk.DestroyDevice(_device, null);

#if DEBUG
        _extDebugUtils?.DestroyDebugUtilsMessenger(_instance, _debugMessenger, null);
#endif

        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();

        foreach (var window in _windows) {
            window.SilkWindow.Close();
            window.SilkWindow.Dispose();
        }
    }

    public bool Update()
    {
        foreach (var window in _windows) {
            window.SilkWindow.DoEvents();
        }

        var primary = _windows.FirstOrDefault(w => w.IsPrimary);

        return primary is { SilkWindow.IsClosing: false };
    }

    public FrameTimer CreateFrameTimer()
    {
        return new FrameTimer();
    }

    public DuckIWindow CreateWindow(in WindowConfiguration? config = null)
    {
        var window = CreateWindowInternal(config ?? WindowConfiguration.Default, false);

        CreateSurface(window);
        CreateSwapchain(window);
        WindowCreated?.Invoke(window);

        return window;
    }

    private VulkanWindow CreateWindowInternal(WindowConfiguration config, bool isPrimary)
    {
        if (!_windowingRegistered) {
            SdlWindowing.Use();
            _windowingRegistered = true;
        }

        var options = WindowOptions.DefaultVulkan with {
            Title = config.Title,
            Size = config.Dimensions,
            IsVisible = true,
        };

        var silkWindow = Window.Create(options);
        silkWindow.Initialize();
        silkWindow.DoEvents();

        var window = new VulkanWindow(silkWindow, isPrimary);
        _windows.Add(window);

        return window;
    }

    private bool CreateInstance(VulkanWindow primaryWindow)
    {
        if (primaryWindow.SilkWindow.VkSurface is null) {
            _logger.LogError("Window does not support Vulkan surfaces");
            return false;
        }

        var glfwExtensions = primaryWindow.SilkWindow.VkSurface.GetRequiredExtensions(out var glfwExtensionCount);
        var extensions = new List<string>();

        for (var i = 0; i < (int)glfwExtensionCount; i++) {
            extensions.Add(Marshal.PtrToStringAnsi((nint)glfwExtensions[i]) ?? string.Empty);
        }

#if DEBUG
        extensions.Add(ExtDebugUtils.ExtensionName);
        var validationLayers = GetAvailableValidationLayers(["VK_LAYER_KHRONOS_validation"]);
#else
        var validationLayers = [];
#endif

        using var pAppName = SilkMarshal.StringToMemory("Duck");
        using var pEngineName = SilkMarshal.StringToMemory("Duck Engine");

        var appInfo = new ApplicationInfo {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)pAppName,
            ApplicationVersion = new Version32(0, 1, 0),
            PEngineName = (byte*)pEngineName,
            EngineVersion = new Version32(0, 1, 0),
            ApiVersion = Vk.Version13,
        };

        using var pExtensions = SilkMarshal.StringArrayToMemory(extensions.ToArray());
        using var pLayers = SilkMarshal.StringArrayToMemory(validationLayers);

        var createInfo = new InstanceCreateInfo {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Count,
            PpEnabledExtensionNames = (byte**)pExtensions,
            EnabledLayerCount = (uint)validationLayers.Length,
            PpEnabledLayerNames = validationLayers.Length > 0 ? (byte**)pLayers : null,
        };

        if (_vk!.CreateInstance(&createInfo, null, out _instance) != Result.Success) {
            _logger.LogError("Failed to create Vulkan instance");
            return false;
        }

        _logger.LogInformation("Vulkan instance created");

        return true;
    }

    private string[] GetAvailableValidationLayers(string[] requested)
    {
        uint count = 0;

        _vk!.EnumerateInstanceLayerProperties(&count, null);

        var available = new LayerProperties[count];

        fixed(LayerProperties* pLayers = available) {
            _vk.EnumerateInstanceLayerProperties(&count, pLayers);
        }

        var availableNames = new HashSet<string>();

        foreach (var t in available) {
            availableNames.Add(Marshal.PtrToStringAnsi((nint)t.LayerName) ?? string.Empty);
        }

        var result = requested
            .Where(availableNames.Contains)
            .ToArray();

        foreach (var missing in requested.Except(result)) {
            _logger.LogWarning("Validation layer {0} not available", missing);
        }

        return result;
    }

#if DEBUG
    private void SetupDebugMessenger()
    {
        if (!_vk!.TryGetInstanceExtension<ExtDebugUtils>(_instance, out _extDebugUtils)) {
            return;
        }

        _debugCallback = new PfnDebugUtilsMessengerCallbackEXT(DebugCallback);

        var createInfo = new DebugUtilsMessengerCreateInfoEXT {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt
                              | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt
                              | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt
                          | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt
                          | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
            PfnUserCallback = _debugCallback,
        };

        if (_extDebugUtils.CreateDebugUtilsMessenger(_instance, &createInfo, null, out _debugMessenger)
            != Result.Success) {
            _logger.LogWarning("Failed to setup Vulkan debug messenger");
        }
    }

    private static uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT severity,
        DebugUtilsMessageTypeFlagsEXT types,
        DebugUtilsMessengerCallbackDataEXT* data,
        void* userData)
    {
        if (severity < DebugUtilsMessageSeverityFlagsEXT.WarningBitExt) {
            return Vk.False;
        }

        var message = Marshal.PtrToStringAnsi((nint)data->PMessage);
        Console.Error.WriteLine($"[Vulkan] {message}");

        return Vk.False;
    }
#endif

    private bool CreateSurface(VulkanWindow window)
    {
        var surface = window.SilkWindow.VkSurface!
            .Create<AllocationCallbacks>(_instance.ToHandle(), null)
            .ToSurface();

        if (surface.Handle == 0) {
            _logger.LogError("Failed to create Vulkan surface");
            return false;
        }

        window.SetSurface(surface);

        return true;
    }

    private bool SelectPhysicalDevice(SurfaceKHR surface)
    {
        uint deviceCount = 0;

        _vk!.EnumeratePhysicalDevices(_instance, &deviceCount, null);

        if (deviceCount == 0) {
            _logger.LogError("No Vulkan-capable GPU found");
            return false;
        }

        var devices = new PhysicalDevice[deviceCount];

        fixed(PhysicalDevice* pDevices = devices) {
            _vk.EnumeratePhysicalDevices(_instance, &deviceCount, pDevices);
        }

        var best = default(PhysicalDevice);
        var bestScore = -1;

        foreach (var device in devices) {
            _vk.GetPhysicalDeviceProperties(device, out var props);
            var name = Marshal.PtrToStringAnsi((nint)props.DeviceName) ?? "Unknown";

            if (!IsDeviceSuitable(device, surface, name)) {
                continue;
            }

            var score = props.DeviceType == PhysicalDeviceType.DiscreteGpu ? 1000 : 0;

            if (score <= bestScore) {
                continue;
            }

            best = device;
            bestScore = score;
        }

        if (bestScore < 0) {
            _logger.LogError("No suitable Vulkan GPU found");
            return false;
        }

        _physicalDevice = best;

        _vk.GetPhysicalDeviceProperties(_physicalDevice, out var selectedProps);

        var deviceName = Marshal.PtrToStringAnsi((nint)selectedProps.DeviceName) ?? "Unknown";

        _logger.LogInformation("Selected GPU: {0} ({1})", deviceName, selectedProps.DeviceType);

        GetQueueFamilies(_physicalDevice, surface, out _graphicsQueueFamily, out _presentQueueFamily);

        _supportedInstanceExtensions = GetSupportedInstanceExtensions(_physicalDevice);
        _supportedDeviceExtensions = GetSupportedDeviceExtensions(_physicalDevice);

        _logger.LogInformation($"Supported extensions: {string.Join(',', _supportedDeviceExtensions)}");

        return true;
    }

    private bool IsDeviceSuitable(PhysicalDevice device, SurfaceKHR surface, string name)
    {
        GetQueueFamilies(device, surface, out var graphicsFamily, out var presentFamily);

        if (graphicsFamily == uint.MaxValue) {
            _logger.LogError("GPU {0}: no graphics queue family", name);
            return false;
        }

        if (presentFamily == uint.MaxValue) {
            _logger.LogError("GPU {0}: no present queue family for surface", name);
            return false;
        }

        if (!HasRequiredExtensions(device)) {
            return false;
        }
        return false;
        uint formatCount = 0;
        _khrSurface!.GetPhysicalDeviceSurfaceFormats(device, surface, &formatCount, null);

        uint presentModeCount = 0;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(device, surface, &presentModeCount, null);

        if (formatCount == 0 || presentModeCount == 0) {
            _logger.LogError(
                "GPU {0}: inadequate swapchain (formats={1}, modes={2})",
                name,
                formatCount,
                presentModeCount
            );
            return false;
        }

        return true;
    }

    private bool HasRequiredExtensions(PhysicalDevice device)
    {
        string[] required = [
            KhrSwapchain.ExtensionName,
            "VK_EXT_memory_budget",
        ];

        var supportedExtensions = GetSupportedDeviceExtensions(device);

        var hasAllExtensions = true;

        foreach (var extName in required) {
            if (!supportedExtensions.Contains(extName)) {
                _logger.LogError($"GPU {0}: missing {extName}");
                hasAllExtensions = false;
            }
        }

        return hasAllExtensions;
    }

    private void GetQueueFamilies(PhysicalDevice device,
        SurfaceKHR surface,
        out uint graphicsFamily,
        out uint presentFamily)
    {
        graphicsFamily = uint.MaxValue;
        presentFamily = uint.MaxValue;

        uint count = 0;
        _vk!.GetPhysicalDeviceQueueFamilyProperties(device, &count, null);

        var families = new QueueFamilyProperties[count];

        fixed(QueueFamilyProperties* pFamilies = families) {
            _vk.GetPhysicalDeviceQueueFamilyProperties(device, &count, pFamilies);
        }

        for (uint i = 0; i < families.Length; i++) {
            if (families[i]
                .QueueFlags.HasFlag(QueueFlags.GraphicsBit)) {
                graphicsFamily = i;
            }

            _khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

            if (presentSupport) {
                presentFamily = i;
            }

            if (graphicsFamily != uint.MaxValue && presentFamily != uint.MaxValue) {
                break;
            }
        }
    }

    private bool CreateLogicalDevice()
    {
        var uniqueFamilies = new HashSet<uint> { _graphicsQueueFamily, _presentQueueFamily };
        var queueCreateInfos = new DeviceQueueCreateInfo[uniqueFamilies.Count];
        var queuePriority = 1.0f;

        var i = 0;

        foreach (var family in uniqueFamilies) {
            queueCreateInfos[i++] = new DeviceQueueCreateInfo {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = family,
                QueueCount = 1,
                PQueuePriorities = &queuePriority,
            };
        }

        var features = new PhysicalDeviceFeatures();
        var dynamicRenderingFeatures = new PhysicalDeviceDynamicRenderingFeatures {
            SType = StructureType.PhysicalDeviceDynamicRenderingFeatures,
            DynamicRendering = true,
        };

        using var pSwapchainExt = SilkMarshal.StringToMemory(KhrSwapchain.ExtensionName);
        var enabledExtensions = stackalloc byte*[] { (byte*)pSwapchainExt };

        fixed(DeviceQueueCreateInfo* pQueueCreateInfos = queueCreateInfos) {
            var createInfo = new DeviceCreateInfo {
                SType = StructureType.DeviceCreateInfo,
                PNext = &dynamicRenderingFeatures,
                QueueCreateInfoCount = (uint)queueCreateInfos.Length,
                PQueueCreateInfos = pQueueCreateInfos,
                PEnabledFeatures = &features,
                EnabledExtensionCount = 1,
                PpEnabledExtensionNames = enabledExtensions,
            };

            if (_vk!.CreateDevice(_physicalDevice, &createInfo, null, out _device) != Result.Success) {
                _logger.LogError("Failed to create logical device");
                return false;
            }
        }

        _vk.GetDeviceQueue(_device, _graphicsQueueFamily, 0, out _graphicsQueue);
        _vk.GetDeviceQueue(_device, _presentQueueFamily, 0, out _presentQueue);

        _logger.LogInformation("Logical device created");

        return true;
    }

    private bool CreateSwapchain(VulkanWindow window)
    {
        _khrSurface!.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, window.Surface, out var capabilities);

        uint formatCount = 0;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, window.Surface, &formatCount, null);

        var formats = new SurfaceFormatKHR[formatCount];

        fixed(SurfaceFormatKHR* pFormats = formats) {
            _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, window.Surface, &formatCount, pFormats);
        }

        uint presentModeCount = 0;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, window.Surface, &presentModeCount, null);

        var presentModes = new PresentModeKHR[presentModeCount];

        fixed(PresentModeKHR* pModes = presentModes) {
            _khrSurface.GetPhysicalDeviceSurfacePresentModes(
                _physicalDevice,
                window.Surface,
                &presentModeCount,
                pModes
            );
        }

        var surfaceFormat = ChooseSurfaceFormat(formats);
        var presentMode = ChoosePresentMode(presentModes);
        var extent = ChooseSwapExtent(capabilities, window);
        var imageCount = capabilities.MinImageCount + 1;

        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount) {
            imageCount = capabilities.MaxImageCount;
        }

        var createInfo = new SwapchainCreateInfoKHR {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = window.Surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true,
        };

        var families = stackalloc uint[] { _graphicsQueueFamily, _presentQueueFamily };

        if (_graphicsQueueFamily != _presentQueueFamily) {
            createInfo.ImageSharingMode = SharingMode.Concurrent;
            createInfo.QueueFamilyIndexCount = 2;
            createInfo.PQueueFamilyIndices = families;
        } else {
            createInfo.ImageSharingMode = SharingMode.Exclusive;
        }

        if (_khrSwapchain!.CreateSwapchain(_device, &createInfo, null, out var swapchain) != Result.Success) {
            _logger.LogError("Failed to create swapchain");
            return false;
        }

        uint swapImageCount = 0;
        _khrSwapchain.GetSwapchainImages(_device, swapchain, &swapImageCount, null);

        var images = new Image[swapImageCount];

        fixed(Image* pImages = images) {
            _khrSwapchain.GetSwapchainImages(_device, swapchain, &swapImageCount, pImages);
        }

        var imageViews = new ImageView[swapImageCount];

        for (var i = 0; i < images.Length; i++) {
            var viewCreateInfo = new ImageViewCreateInfo {
                SType = StructureType.ImageViewCreateInfo,
                Image = images[i],
                ViewType = ImageViewType.Type2D,
                Format = surfaceFormat.Format,
                Components = new ComponentMapping {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange = new ImageSubresourceRange {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
            };

            if (_vk!.CreateImageView(_device, &viewCreateInfo, null, out imageViews[i]) != Result.Success) {
                _logger.LogError($"Failed to create swapchain image view {i}");
            }
        }

        window.SetSwapchain(swapchain, surfaceFormat.Format, extent, images, imageViews);

        _logger.LogInformation(
            "Swapchain created: {0}x{1}, {2} images, format {3}",
            extent.Width,
            extent.Height,
            swapImageCount,
            surfaceFormat.Format
        );

        return true;
    }

    private void DestroyWindowResources(VulkanWindow window)
    {
        foreach (var view in window.SwapchainImageViews) {
            _vk!.DestroyImageView(_device, view, null);
        }

        if (window.Swapchain.Handle != 0) {
            _khrSwapchain!.DestroySwapchain(_device, window.Swapchain, null);
        }

        if (window.Surface.Handle != 0) {
            _khrSurface!.DestroySurface(_instance, window.Surface, null);
        }
    }

    private static SurfaceFormatKHR ChooseSurfaceFormat(SurfaceFormatKHR[] formats)
    {
        foreach (var format in formats) {
            if (format is { Format: Format.B8G8R8A8Srgb, ColorSpace: ColorSpaceKHR.SpaceSrgbNonlinearKhr }) {
                return format;
            }
        }

        foreach (var format in formats) {
            if (format is { Format: Format.R8G8B8A8Srgb, ColorSpace: ColorSpaceKHR.SpaceSrgbNonlinearKhr }) {
                return format;
            }
        }

        return formats[0];
    }

    private static PresentModeKHR ChoosePresentMode(PresentModeKHR[] presentModes)
    {
        if (presentModes.Contains(PresentModeKHR.MailboxKhr)) {
            return PresentModeKHR.MailboxKhr;
        }

        return PresentModeKHR.FifoKhr;
    }

    private static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities, VulkanWindow window)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue) {
            return capabilities.CurrentExtent;
        }

        return new Extent2D(
            Math.Clamp((uint)window.Dimensions.X, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
            Math.Clamp(
                (uint)window.Dimensions.Y,
                capabilities.MinImageExtent.Height,
                capabilities.MaxImageExtent.Height
            )
        );
    }

    private HashSet<string> GetSupportedDeviceExtensions(PhysicalDevice device)
    {
        uint count = 0;
        _vk!.EnumerateDeviceExtensionProperties(device, (byte*)null, &count, null);

        var extensions = new ExtensionProperties[count];

        fixed(ExtensionProperties* pExtensions = extensions) {
            _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, &count, pExtensions);
        }

        var supported = new HashSet<string>((int)count, StringComparer.Ordinal);

        foreach (var t in extensions) {
            var name = Marshal.PtrToStringAnsi((nint)t.ExtensionName);

            if (!string.IsNullOrEmpty(name)) {
                supported.Add(name);
            }
        }

        return supported;
    }

    private HashSet<string> GetSupportedInstanceExtensions(PhysicalDevice device)
    {
        uint count = 0;
        _vk!.EnumerateInstanceExtensionProperties((byte*)null, &count, null);

        var extensions = new ExtensionProperties[count];

        fixed(ExtensionProperties* pExtensions = extensions) {
            _vk.EnumerateInstanceExtensionProperties((byte*)null, &count, pExtensions);
        }

        var supported = new HashSet<string>((int)count, StringComparer.Ordinal);

        foreach (var t in extensions) {
            var name = Marshal.PtrToStringAnsi((nint)t.ExtensionName);

            if (!string.IsNullOrEmpty(name)) {
                supported.Add(name);
            }
        }

        return supported;
    }

    private bool SupportsDeviceExtension(string name)
    {
        return _supportedDeviceExtensions?.Contains(name) ?? false;
    }

    private bool SupportsInstanceExtension(string name)
    {
        return _supportedInstanceExtensions?.Contains(name) ?? false;
    }
}