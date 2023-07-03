using System.Collections.Concurrent;
using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using Duck.Content;
using Duck.Renderer.Content.SourceAssetImporter;
using Duck.Renderer.Device;
using Duck.Logging;
using Duck.Platform;
using Duck.Renderer.Components;
using Duck.Renderer.Events;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Renderer;

public class RendererModule : IRendererModule,
    IInitializableModule,
    IPreTickableModule,
    ITickableModule,
    IPostTickableModule,
    IPreRenderableModule,
    IRenderableModule,
    IPostRenderableModule
{
    #region Properties

    public IGraphicsDevice? GraphicsDevice => _renderSystem.GraphicsDevice;
    public IRenderSystem RenderSystem => _renderSystem;
    public View? GameView { get; set; }
    public View[] Views => _views.Values.ToArray();

    private readonly IEventBus _eventBus;
    private readonly ConcurrentDictionary<string, IScene> _loadedScenes = new();
    private readonly ConcurrentDictionary<string, View> _views = new();

    private readonly ConcurrentBag<IScene> _pendingUnload = new();

    #endregion

    #region Members

    private readonly ILogger _logger;
    private readonly IPlatform _platform;
    private readonly IRenderSystem _renderSystem;
    private readonly IApplication _app;
    private readonly IContentModule _contentModule;

    private IWindow? _window;

    #endregion

    #region Methods

    public RendererModule(IApplication app, IPlatform platform, IEventBus eventBus, IRenderSystem renderSystem, ILogModule logModule, IContentModule contentModule)
    {
        _app = app;
        _contentModule = contentModule;
        _platform = platform;
        _renderSystem = renderSystem;
        _eventBus = eventBus;

        _logger = logModule.CreateLogger("Graphics");
        _logger.LogInformation("Created graphics module.");
    }

    #endregion

    #region IRenderingModule

    public bool Init()
    {
        _logger.LogInformation("Initializing graphics module...");

        _window = _platform.CreateWindow();

        _renderSystem.Init(_app, _window);

        _contentModule.RegisterSourceAssetImporter(
            new FbxAssetImporter(
                _app.GetModule<IContentModule>().ContentRootDirectory,
                _renderSystem.FallbackMaterial.MakeSharedReference()
            )
        );

        GameView = CreateView("Game");
        GameView.Position = Vector2D<int>.Zero;
        GameView.Dimensions = new Vector2D<int>(1280, 1024);
        GameView.Position = new Vector2D<int>(0, 0);
        GameView.IsEnabled = true;

        return true;
    }

    public void PreTick()
    {
        foreach (var scene in _pendingUnload) {
            if (!_loadedScenes.ContainsKey(scene.Name)) {
                continue;
            }

            _loadedScenes.Remove(scene.Name, out var unused);
            _pendingUnload.TryTake(out var unused2);

            World.Destroy(scene.World);
        }

        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.PreTick(Time.DeltaFrame);
            }
        }
    }

    public void Tick()
    {
        ProcessWindowEvents();

        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.Tick(Time.DeltaFrame);
            }
        }
    }

    public void PostTick()
    {
        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.PostTick(Time.DeltaFrame);
            }
        }
    }

    public void PreRender()
    {
        _renderSystem.PreRender();
    }

    public void Render()
    {
        foreach (var kvp in _views) {
            var view = kvp.Value;
            var cameraRef = view.Camera;

            if (!view.IsValid) {
                continue;
            }

            Debug.Assert(view.Scene != null);
            Debug.Assert(cameraRef.HasValue);

            view.Scene.TryGetTarget(out var scene);

            // FIXME: this does not support multithreading

            var cameraTransform = cameraRef.Value.Entity.Get<TransformComponent>();

            var commandBuffer = _renderSystem.GraphicsDevice.CreateCommandBuffer(view);
            commandBuffer.ViewMatrix =
                Matrix4X4.CreateLookAt(
                    cameraTransform.Position,
                    cameraTransform.Position + cameraTransform.Forward,
                    cameraTransform.Up
                );

            Time.CameraPosition = cameraTransform.Position;

            scene.SystemRoot.PresentationGroup.RenderCommandBuffer = commandBuffer;
            scene.SystemRoot.PresentationGroup.View = view;

            scene.SystemRoot.PresentationGroup.BeforeUpdate(Time.DeltaFrame);
            scene.SystemRoot.PresentationGroup.Update(Time.DeltaFrame);
            scene.SystemRoot.PresentationGroup.AfterUpdate(Time.DeltaFrame);

            _renderSystem.GraphicsDevice.Render(commandBuffer);
        }
    }

    public void PostRender()
    {
        _renderSystem.PostRender();
    }

    private void ProcessWindowEvents()
    {
        if (null == _window) {
            return;
        }

        foreach (var windowEvent in _window.Events) {
            if (windowEvent is ResizeEvent resizeEvent) {
                foreach (var kvp in _views) {
                    ResizeViewToWindow(kvp.Value);
                }
            }
        }
    }

    private void ResizeView(View view, Vector2D<int> newSize)
    {
        view.Dimensions = newSize;
    }

    private void ResizeViewToWindow(View view)
    {
        ResizeView(view, new Vector2D<int>(_window.Width, _window.Height));
    }

    public View CreateView(string name)
    {
        var view = new View(name);

        if (!_views.TryAdd(name, view)) {
            throw new Exception("TODO: errors");
        }

        ResizeViewToWindow(view);

        return view;
    }

    public IScene CreateScene(string name)
    {
        return CreateScene(name, World.Create());
    }

    public IScene CreateScene(string name, World world)
    {
        var scene = new Scene(name, world, _eventBus);

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        _eventBus.Emit(new SceneWasCreated() {
            Scene = scene
        });

        return scene;
    }

    public IScene GetOrCreateScene(string name)
    {
        var scene = GetLoadedScene(name);

        if (null != scene) {
            return scene;
        }

        return CreateScene(name);
    }

    public void UnloadScene(IScene scene)
    {
        scene.IsActive = false;

        if (!_pendingUnload.Contains(scene)) {
            _pendingUnload.Add(scene);
        }
    }

    public IScene[] GetLoadedScenes()
    {
        return _loadedScenes.Values.ToArray();
    }

    public IScene? GetLoadedScene(string name)
    {
        if (_loadedScenes.TryGetValue(name, out var scene)) {
            return scene;
        }

        return null;
    }

    #endregion
}
