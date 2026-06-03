using Duck.ModuleManagement;
using Duck.Platform;
using Schedulers;
using Silk.NET.Vulkan;
using Logger = Duck.Platform.Logging.Logger;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Duck.RenderSystem.Vulkan;

public unsafe class VulkanRenderModule : IInitializableModule, IShutdownModule,
    IPresentSingleThreadedModule
{
    private const int MaxFramesInFlight = 2;

    private readonly Logger _logger;
    private VulkanPlatform? _platform;
    private int _currentFrame;

    private readonly Dictionary<VulkanWindow, WindowRenderData> _windowData = [];
    private JobScheduler? _scheduler;

    public VulkanRenderModule(VulkanPlatform platform, Logger logger)
    {
        _platform = platform;
        _logger = logger;
    }

    public void Initialize(IApplication app, IInitializationContext context)
    {
        if (context.WasHotReloaded) {
            return;
        }

        _scheduler = app.Scheduler;

        foreach (var window in _platform!.Windows) {
            CreateWindowRenderData(window);
        }

        _platform.WindowCreated += OnWindowCreated;

        _logger.LogInformation("Vulkan render module initialized");
    }

    public void Shutdown(IApplication app)
    {
        if (_platform is null) {
            return;
        }

        var vk = _platform.Vk;
        var device = _platform.Device;

        vk.DeviceWaitIdle(device);

        foreach (var (_, data) in _windowData) {
            DestroyWindowRenderData(vk, device, data);
        }

        _windowData.Clear();

        _platform.WindowCreated -= OnWindowCreated;
        _scheduler = null;
        _platform = null;
    }

    public void Present(FrameTimer frameTimer)
    {
        if (_platform is null) {
            return;
        }

        var platform = _platform;
        var entries = _windowData.ToArray();
        var imageIndices = new int[entries.Length];

        // Phase 1: fence wait + command buffer recording, one job per window.
        // Each window has its own command pool so recording is thread-safe.
        Span<JobHandle> handles = stackalloc JobHandle[entries.Length];

        for (var i = 0; i < entries.Length; i++) {
            handles[i] = _scheduler!.Schedule(
                new WindowRenderJob(this, platform, entries[i].Key, entries[i].Value, _currentFrame, imageIndices, i)
            );
        }

        _scheduler!.Flush();
        JobHandle.CompleteAll(handles);

        // Phase 2: submit and present serially — Vulkan queues require external synchronization.
        var vk = platform.Vk;
        for (var i = 0; i < entries.Length; i++) {
            if (imageIndices[i] < 0) {
                continue;
            }

            var (window, data) = entries[i];
            var imageIndex = (uint)imageIndices[i];
            var fence = data.InFlightFences[_currentFrame];
            var waitSemaphore = data.ImageAvailableSemaphores[_currentFrame];
            var signalSemaphore = data.RenderFinishedSemaphores[imageIndex];
            var waitStage = PipelineStageFlags.ColorAttachmentOutputBit;
            var cmdBuffer = data.CommandBuffers[imageIndex];

            var submitInfo = new SubmitInfo {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &waitSemaphore,
                PWaitDstStageMask = &waitStage,
                CommandBufferCount = 1,
                PCommandBuffers = &cmdBuffer,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = &signalSemaphore,
            };

            vk.QueueSubmit(platform.GraphicsQueue, 1, &submitInfo, fence);

            var swapchain = window.Swapchain;

            var presentInfo = new PresentInfoKHR {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &signalSemaphore,
                SwapchainCount = 1,
                PSwapchains = &swapchain,
                PImageIndices = &imageIndex,
            };

            platform.KhrSwapchain.QueuePresent(platform.PresentQueue, &presentInfo);
        }

        _currentFrame = (_currentFrame + 1) % MaxFramesInFlight;
    }

    // Returns the acquired image index, or -1 if the window should be skipped this frame.
    // Called from a scheduler thread — only touches per-window data and its own command pool.
    private int Parallel_WaitAndRecord(VulkanPlatform platform,
        VulkanWindow window,
        WindowRenderData data,
        int frameIndex)
    {
        var vk = platform.Vk;
        var device = platform.Device;
        var fence = data.InFlightFences[frameIndex];

        vk.WaitForFences(device, 1, &fence, true, ulong.MaxValue);

        uint imageIndex;
        var result = platform.KhrSwapchain.AcquireNextImage(
            device,
            window.Swapchain,
            ulong.MaxValue,
            data.ImageAvailableSemaphores[frameIndex],
            default,
            &imageIndex
        );

        if (result != Result.Success && result != Result.SuboptimalKhr) {
            return -1;
        }

        if (data.ImagesInFlight[imageIndex].Handle != 0) {
            var imgFence = data.ImagesInFlight[imageIndex];
            vk.WaitForFences(device, 1, &imgFence, true, ulong.MaxValue);
        }

        data.ImagesInFlight[imageIndex] = fence;

        vk.ResetFences(device, 1, &fence);
        vk.ResetCommandBuffer(data.CommandBuffers[imageIndex], 0);
        RecordCommandBuffer(data.CommandBuffers[imageIndex], imageIndex, vk, window);

        return (int)imageIndex;
    }

    private readonly struct WindowRenderJob : IJob
    {
        private readonly VulkanRenderModule _module;
        private readonly VulkanPlatform _platform;
        private readonly VulkanWindow _window;
        private readonly WindowRenderData _data;
        private readonly int _frameIndex;
        private readonly int[] _imageIndices;
        private readonly int _index;

        public WindowRenderJob(VulkanRenderModule module,
            VulkanPlatform platform,
            VulkanWindow window,
            WindowRenderData data,
            int frameIndex,
            int[] imageIndices,
            int index)
        {
            _module = module;
            _platform = platform;
            _window = window;
            _data = data;
            _frameIndex = frameIndex;
            _imageIndices = imageIndices;
            _index = index;
        }

        public void Execute()
        {
            _imageIndices[_index] = _module.Parallel_WaitAndRecord(_platform, _window, _data, _frameIndex);
        }
    }

    private void OnWindowCreated(VulkanWindow window)
    {
        CreateWindowRenderData(window);
    }

    private void RecordCommandBuffer(CommandBuffer cmd,
        uint imageIndex,
        Vk vk,
        VulkanWindow window)
    {
        var beginInfo = new CommandBufferBeginInfo {
            SType = StructureType.CommandBufferBeginInfo,
        };

        vk.BeginCommandBuffer(cmd, &beginInfo);

        var imageBarrier = new ImageMemoryBarrier {
            SType = StructureType.ImageMemoryBarrier,
            SrcAccessMask = AccessFlags.None,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.ColorAttachmentOptimal,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = window.SwapchainImages[imageIndex],
            SubresourceRange = new ImageSubresourceRange {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1,
            },
        };

        vk.CmdPipelineBarrier(cmd,
            PipelineStageFlags.TopOfPipeBit,
            PipelineStageFlags.ColorAttachmentOutputBit,
            DependencyFlags.None,
            0,
            null,
            0,
            null,
            1,
            &imageBarrier
        );

        var clearValue = new ClearValue { Color = new ClearColorValue(0.1f, 0.1f, 0.1f, 1.0f) };
        var colorAttachment = new RenderingAttachmentInfo {
            SType = StructureType.RenderingAttachmentInfo,
            ImageView = window.SwapchainImageViews[imageIndex],
            ImageLayout = ImageLayout.ColorAttachmentOptimal,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            ClearValue = clearValue,
        };

        var renderingInfo = new RenderingInfo {
            SType = StructureType.RenderingInfo,
            RenderArea = new Rect2D { Offset = new Offset2D(0, 0), Extent = window.SwapchainExtent },
            LayerCount = 1,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachment,
        };

        vk.CmdBeginRendering(cmd, &renderingInfo);
        vk.CmdEndRendering(cmd);

        imageBarrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
        imageBarrier.DstAccessMask = AccessFlags.None;
        imageBarrier.OldLayout = ImageLayout.ColorAttachmentOptimal;
        imageBarrier.NewLayout = ImageLayout.PresentSrcKhr;

        vk.CmdPipelineBarrier(cmd,
            PipelineStageFlags.ColorAttachmentOutputBit,
            PipelineStageFlags.BottomOfPipeBit,
            DependencyFlags.None,
            0,
            null,
            0,
            null,
            1,
            &imageBarrier
        );

        vk.EndCommandBuffer(cmd);
    }

    private void CreateWindowRenderData(VulkanWindow window)
    {
        var data = new WindowRenderData();

        CreateCommandPool(data);
        CreateCommandBuffers(window, data);
        CreateSyncObjects(window, data);

        _windowData[window] = data;
    }

    private void DestroyWindowRenderData(Vk vk, Device device, WindowRenderData data)
    {
        for (var i = 0; i < data.ImageAvailableSemaphores.Length; i++) {
            vk.DestroySemaphore(device, data.ImageAvailableSemaphores[i], null);
        }

        for (var i = 0; i < data.RenderFinishedSemaphores.Length; i++) {
            vk.DestroySemaphore(device, data.RenderFinishedSemaphores[i], null);
        }

        for (var i = 0; i < data.InFlightFences.Length; i++) {
            vk.DestroyFence(device, data.InFlightFences[i], null);
        }

        if (data.CommandPool.Handle != 0) {
            vk.DestroyCommandPool(device, data.CommandPool, null);
        }
    }

    private void CreateCommandPool(WindowRenderData data)
    {
        var poolInfo = new CommandPoolCreateInfo {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = _platform!.GraphicsQueueFamily,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
        };

        if (_platform.Vk.CreateCommandPool(_platform.Device, &poolInfo, null, out data.CommandPool) != Result.Success) {
            throw new InvalidOperationException("Failed to create command pool");
        }
    }

    private void CreateCommandBuffers(VulkanWindow window, WindowRenderData data)
    {
        var vk = _platform!.Vk;

        data.CommandBuffers = new CommandBuffer[window.SwapchainImageViews.Length];

        var allocInfo = new CommandBufferAllocateInfo {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = data.CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)data.CommandBuffers.Length,
        };

        fixed(CommandBuffer* pBuffers = data.CommandBuffers) {
            if (vk.AllocateCommandBuffers(_platform!.Device, &allocInfo, pBuffers) != Result.Success) {
                throw new InvalidOperationException("Failed to allocate command buffers");
            }
        }
    }

    private void CreateSyncObjects(VulkanWindow window, WindowRenderData data)
    {
        var vk = _platform!.Vk;
        var device = _platform.Device;
        var imageCount = window.SwapchainImages.Length;

        data.ImageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
        data.RenderFinishedSemaphores = new Semaphore[imageCount];
        data.InFlightFences = new Fence[MaxFramesInFlight];
        data.ImagesInFlight = new Fence[imageCount];

        var semaphoreInfo = new SemaphoreCreateInfo {
            SType = StructureType.SemaphoreCreateInfo,
        };

        var fenceInfo = new FenceCreateInfo {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit,
        };

        for (var i = 0; i < MaxFramesInFlight; i++) {
            if (vk.CreateSemaphore(device, &semaphoreInfo, null, out data.ImageAvailableSemaphores[i]) != Result.Success
                || vk.CreateFence(device, &fenceInfo, null, out data.InFlightFences[i]) != Result.Success) {
                throw new InvalidOperationException("Failed to create synchronization objects");
            }
        }

        for (var i = 0; i < imageCount; i++) {
            if (vk.CreateSemaphore(device, &semaphoreInfo, null, out data.RenderFinishedSemaphores[i]) != Result.Success) {
                throw new InvalidOperationException("Failed to create synchronization objects");
            }
        }
    }
}
