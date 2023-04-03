using System.Diagnostics;
using System.Reflection;
using ChickenWithLips.WickedEngine;
using Duck.Graphics;
using Duck.Logging;
using Duck.Platform;
using EventHandler = ChickenWithLips.WickedEngine.EventHandler;

namespace Duck.RenderSystems.Wicked;

// largely based on wicked engine base application
public class WickedRenderSystem : IRenderSystem
{
    private ILogger? _logger;
    private GraphicsDevice? _graphicsDevice;
    private SwapChain? _swapChain;
    private RenderPath? _activePath;

    private readonly Canvas _canvas = new SimpleCanvas();

    public void Init(IApplication app, IWindow window)
    {
        _logger = app.GetModule<ILogModule>().CreateLogger("WickedRenderSystem");

        Console.WriteLine("FIXME: Wicked arguments");

        ValidationMode validationMode = ValidationMode.Disabled;

        // User can also create a graphics device if custom logic is desired, but they must do before this function!
// 		if (graphicsDevice == nullptr)
// 		{
// 			ValidationMode validationMode = ValidationMode::Disabled;
// 			if (wi::arguments::HasArgument("debugdevice"))
// 			{
// 				validationMode = ValidationMode::Enabled;
// 			}
// 			if (wi::arguments::HasArgument("gpuvalidation"))
// 			{
// 				validationMode = ValidationMode::GPU;
// 			}
// 			if (wi::arguments::HasArgument("gpu_verbose"))
// 			{
        validationMode = ValidationMode.Verbose;
// 			}
//
        bool shouldUseDx12 = true; //wi::arguments::HasArgument("dx12");
        bool shouldUseVulkan = false; //wi::arguments::HasArgument("vulkan");

        if (!Build.SupportsDirectX12 && shouldUseDx12) {
            _logger.LogError("DX12 is not supported in this build.");
            shouldUseDx12 = false;
        }

        if (!Build.SupportsVulkan && shouldUseVulkan) {
            _logger.LogError("Vulkan is not supported in this build.");
            shouldUseVulkan = false;
        }

        if (!shouldUseDx12 && !shouldUseVulkan) {
            if (Build.SupportsDirectX12) {
                shouldUseDx12 = true;
            } else if (Build.SupportsVulkan) {
                shouldUseVulkan = true;
            } else {
                _logger.LogError("No rendering backend is enabled.");
                // FIXME: this should shutdown the app
            }
        }

        Debug.Assert(shouldUseDx12 || shouldUseVulkan);


        if (shouldUseVulkan && Build.SupportsVulkan) {
            Renderer.ShaderPath = Path.Combine(Renderer.ShaderPath, "spirv/");
            _graphicsDevice = new VulkanGraphicsDevice(window.Handle, validationMode);
        } else if (shouldUseDx12 && Build.SupportsDirectX12) {
            Renderer.ShaderPath = Path.Combine(Renderer.ShaderPath, "hlsl6/");
            _graphicsDevice = new DX12GraphicsDevice(validationMode);
        }

        Console.WriteLine("FIXME: ShaderSourcePath");
        Renderer.ShaderSourcePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "shaders") + Path.DirectorySeparatorChar;
        ChickenWithLips.WickedEngine.Graphics.Device = _graphicsDevice;

        _canvas.Init(window.Handle);

        var swapChainDesc = new SwapChainDescription() {
            BufferCount = 3,
            Format = Format.R10G10B10A2_UnsignedNormal,
            Width = _canvas.PhysicalWidth,
            Height = _canvas.PhysicalHeight,
            AllowHdr = true,
        };


        _swapChain = _graphicsDevice.CreateSwapChain(swapChainDesc, window.Handle);

        Debug.Assert(_swapChain != null);

        Console.WriteLine("FIXME: vsync change event");
//
// 		swapChainVsyncChangeEvent = wi::eventhandler::Subscribe(wi::eventhandler::EVENT_SET_VSYNC, [this](uint64_t userdata) {
// 			SwapChainDesc desc = swapChain.desc;
// 			desc.vsync = userdata != 0;
// 			bool success = graphicsDevice->CreateSwapChain(&desc, nullptr, &swapChain);
// 			assert(success);
// 			});
//

        if (_graphicsDevice.GetSwapChainColorSpace(_swapChain) == ColorSpace.Hdr10_ST2084) {
            Console.WriteLine("FIXME: ");
// 			TextureDesc desc;
// 			desc.width = swapChain.desc.width;
// 			desc.height = swapChain.desc.height;
// 			desc.format = Format::R11G11B10_FLOAT;
// 			desc.bind_flags = BindFlag::RENDER_TARGET | BindFlag::SHADER_RESOURCE;
// 			bool success = graphicsDevice->CreateTexture(&desc, nullptr, &rendertarget);
// 			assert(success);
// 			graphicsDevice->SetName(&rendertarget, "Application::rendertarget");
        }

        Initializer.InitializeComponentsImmediate();

        ChickenWithLips.WickedEngine.Scene.GlobalScene.CreateCube("cube");
    }

    public void PreRender()
    {
        Font.UpdateAtlas();

        if (_graphicsDevice == null || _swapChain == null) {
            return;
        }

        var colorSpace = _graphicsDevice.GetSwapChainColorSpace(_swapChain);

        // #ifdef WICKEDENGINE_BUILD_DX12
        // static bool startup_workaround = false;
        // if (!startup_workaround)
        // {
        // 	startup_workaround = true;
        // 	if (dynamic_cast<GraphicsDevice_DX12*>(graphicsDevice.get()))
        // 	{
        // 		CommandList cmd = graphicsDevice->BeginCommandList();
        // 		wi::renderer::Workaround(1, cmd);
        // 		graphicsDevice->SubmitCommandLists();
        // 	}
        // }
        // #endif // WICKEDENGINE_BUILD_DX12

        EventHandler.FireEvent(EventHandler.EventThreadSafePoint, 0);

        if (_activePath != null) {
            _activePath.ColorSpace = colorSpace;
            _activePath.Init(_canvas);
            _activePath.PreUpdate();
        }
    }

    public void Render()
    {
        _activePath?.Update(Time.DeltaFrame);
        _activePath?.Render();
    }

    public void PostRender()
    {
        if (_graphicsDevice == null) {
            return;
        }

        // Begin final compositing

        var cmd = _graphicsDevice.BeginCommandList();

        Image.SetCanvas(_canvas);
        Font.SetCanvas(_canvas);

        var viewport = new Viewport() {
            Height = _swapChain.Description.Height,
            Width = _swapChain.Description.Width,
        };

        _graphicsDevice.BindViewports(1, new[] { viewport }, cmd);

        var colorSpace = _graphicsDevice.GetSwapChainColorSpace(_swapChain);
        var isColorSpaceConversionRequired = colorSpace == ColorSpace.Hdr10_ST2084;

        if (isColorSpaceConversionRequired) {
            // In HDR10, we perform the compositing in a custom linear color space render target
            Console.WriteLine("TODO: color space conversion");
            // RenderPassImage rp[] =  {
            // RenderPassImage::RenderTarget(&rendertarget, RenderPassImage::LoadOp::CLEAR),
            // }
            // ;
            // graphicsDevice->RenderPassBegin(rp, arraysize(rp), cmd);
        } else {
            // If swapchain is SRGB or Linear HDR, it can be used for blending
            // - If it is SRGB, the render path will ensure tonemapping to SDR
            // - If it is Linear HDR, we can blend trivially in linear space
            _graphicsDevice.RenderPassBegin(_swapChain, cmd);
        }

        _activePath?.Compose(cmd);
        _graphicsDevice.RenderPassEnd(cmd);

        if (isColorSpaceConversionRequired) {
            Console.WriteLine("TODO: isColorSpaceConversionRequired");
            // In HDR10, we perform a final mapping from linear to HDR10, into the swapchain
            // graphicsDevice->RenderPassBegin(&swapChain, cmd);
            // wi::image::Params fx;
            // fx.enableFullScreen();
            // fx.enableHDR10OutputMapping();
            // wi::image::Draw(&rendertarget, fx, cmd);
            // graphicsDevice->RenderPassEnd(cmd);
        }

        _graphicsDevice.SubmitCommandLists();
    }
}
