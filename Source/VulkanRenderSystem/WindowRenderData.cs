using Silk.NET.Vulkan;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Duck.RenderSystem.Vulkan;

internal sealed class WindowRenderData
{
    internal CommandPool CommandPool;
    internal CommandBuffer[] CommandBuffers = [];
    internal Semaphore[] ImageAvailableSemaphores = [];
    internal Semaphore[] RenderFinishedSemaphores = [];
    internal Fence[] InFlightFences = [];
    internal Fence[] ImagesInFlight = [];
}
