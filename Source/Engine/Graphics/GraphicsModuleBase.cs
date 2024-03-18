using System.Collections.Concurrent;
using System.Diagnostics;
using Arch.Core;
using Arch.Core.Extensions;
using Duck.Platform;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Events;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Graphics;

public abstract class GraphicsModuleBase : IRendererModule,
    IPreTickableModule,
    ITickableModule,
    IPostTickableModule,
    IRenderableModule,
    IEnterPlayModeModule,
    IExitPlayModeModule
{
    #region Properties

    public virtual IGraphicsDevice GraphicsDevice { get; }
    public virtual IRenderSystem RenderSystem { get; }
    public virtual View PrimaryView { get; set; }

    public View[] Views => _views.Values.ToArray();
    public IScene[] Scenes => _loadedScenes.Values.ToArray();

    protected IEventBus EventBus => _eventBus;

    #endregion

    #region Members

    private bool _isInPlayMode = false;

    private readonly IEventBus _eventBus;
    private readonly ConcurrentDictionary<string, IScene> _pendingUnload = new();
    private readonly ConcurrentDictionary<string, View> _views = new();
    private readonly ConcurrentDictionary<string, IScene> _loadedScenes = new();

    #endregion

    #region Methods

    public GraphicsModuleBase(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void PreTick()
    {
        foreach (var p in _pendingUnload) {
            if (!_loadedScenes.ContainsKey(p.Key)) {
                continue;
            }

            UnloadSceneNow(p.Value);
        }

        _pendingUnload.Clear();

        foreach (var kvp in _loadedScenes) {
            if (kvp.Value.IsActive) {
                kvp.Value.PreTick(Time.DeltaFrame);
            }
        }
    }

    public virtual void Tick()
    {
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

            var scene = view.Scene;

            // FIXME: this does not support multithreading

            var cameraTransform = scene.World.Get<TransformComponent>(cameraRef.Value.Entity);

            var commandBuffer = RenderSystem.GraphicsDevice.CreateCommandBuffer(view);
            commandBuffer.ViewMatrix = cameraTransform.CreateLookAtMatrix();

            Time.CameraPosition = cameraTransform.Position;

            scene.SystemRoot.PresentationGroup.RenderCommandBuffer = commandBuffer;
            scene.SystemRoot.PresentationGroup.View = view;

            scene.SystemRoot.PresentationGroup.BeforeUpdate(Time.DeltaFrame);
            scene.SystemRoot.PresentationGroup.Update(Time.DeltaFrame);
            scene.SystemRoot.PresentationGroup.AfterUpdate(Time.DeltaFrame);

            RenderSystem.GraphicsDevice.Render(commandBuffer);
        }
    }

    public virtual void EnterPlayMode()
    {
        _isInPlayMode = true;

        foreach (var p in _loadedScenes) {
            // FIXME: replace with generators for attaching systems
            EventBus.Emit(new SceneEnteredPlayMode() {
                Scene = p.Value,
            });
        }
    }

    public virtual void ExitPlayMode()
    {
        _isInPlayMode = false;
    }

    protected void ResizeView(View view, Vector2D<int> newSize)
    {
        view.Dimensions = newSize;
    }

    protected void ResizeViewToWindow(View view)
    {
        Console.WriteLine("FIXME: ResizeViewToWindow");
        // ResizeView(view, new Vector2D<int>(_window.Width, _window.Height));
        ResizeView(view, new Vector2D<int>(1280, 1024));
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

    public IScene CreateScene(string name, World? world = null)
    {
        var scene = new Scene(name, world ?? World.Create(), _eventBus);

        if (!_loadedScenes.TryAdd(name, scene)) {
            throw new Exception("TODO: errors");
        }

        _eventBus.Emit(new SceneWasCreated() {
            Scene = scene
        });

        if (_isInPlayMode) {
            Console.WriteLine("EMIT");
            // FIXME: replace with generators for attaching systems
            EventBus.Emit(new SceneEnteredPlayMode() {
                Scene = scene,
            });
        }

        return scene;
    }

    public IScene GetOrCreateScene(string name)
    {
        var scene = FindScene(name);

        if (null != scene) {
            return scene;
        }

        return CreateScene(name);
    }

    public void UnloadScene(IScene scene)
    {
        scene.IsActive = false;

        _pendingUnload.TryAdd(scene.Name, scene);
    }

    public void UnloadSceneNow(IScene scene)
    {
        foreach (var view in Views) {
            if (view.Scene == scene) {
                view.ClearSceneReference();
            }
        }

        _loadedScenes.TryRemove(scene.Name, out var unused);
        _pendingUnload.TryRemove(scene.Name, out var unused2);

        scene.SystemRoot.Dispose();

        World.Destroy(scene.World);
        GC.Collect();
    }

    public IScene? FindScene(string name)
    {
        if (_loadedScenes.TryGetValue(name, out var scene)) {
            return scene;
        }

        return null;
    }

    public View? FindViewForScene(IScene scene)
    {
        foreach (var kvp in _views) {
            if (kvp.Value.Scene == scene) {
                return kvp.Value;
            }
        }

        return null;
    }

    protected void ReplaceScenes(IScene[] scenes)
    {
        foreach (var p in _loadedScenes) {
            foreach (var replacement in scenes) {
                if (p.Value.Name == replacement.Name) {
                    UnloadSceneNow(p.Value);
                }
            }
        }

        foreach (var scene in scenes) {
            _loadedScenes.TryAdd(scene.Name, scene);
        }
    }

    #endregion
}
