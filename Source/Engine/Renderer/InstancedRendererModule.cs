using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Duck.Renderer.Device;
using Duck.Renderer.Events;
using Duck.ServiceBus;

namespace Duck.Renderer;

public class InstancedRendererModule : RendererModuleBase,
    IInitializableModule,
    IEnterPlayModeModule,
    IExitPlayModeModule
{
    #region Properties

    public override IGraphicsDevice GraphicsDevice => _parent.GraphicsDevice;
    public override IRenderSystem RenderSystem => _parent.RenderSystem;

    #endregion

    #region Members

    private readonly IRendererModule _parent;
    private readonly IApplication _app;

    private readonly List<IScene> _sceneBackup = new();
    private readonly Dictionary<View, SceneBackup> _sceneToViewBackup = new();

    #endregion


    public InstancedRendererModule(IRendererModule parent, IApplication app)
        : base(app.GetModule<IEventBus>())
    {
        _parent = parent;
        _app = app;
    }

    public bool Init()
    {
        PrimaryView = CreateView("Primary");

        return true;
    }

    public override void EnterPlayMode()
    {
        foreach (var scene in Scenes) {
            var clone = (IScene)scene.Clone();
            _sceneBackup.Add(clone);

            foreach (var view in Views) {
                if (view.Scene == scene) {
                    _sceneToViewBackup.Add(
                        view,
                        new SceneBackup {
                            Scene = clone,
                            Camera = view.Camera,
                        }
                    );
                }
            }
        }

        base.EnterPlayMode();
    }

    public override void ExitPlayMode()
    {
        ReplaceScenes(_sceneBackup.ToArray());

        foreach (var p in _sceneToViewBackup) {
            p.Key.Scene = p.Value.Scene;
            p.Key.Camera = p.Value.Camera;
        }

        _sceneBackup.Clear();
        _sceneToViewBackup.Clear();

        base.ExitPlayMode();
    }

    private struct SceneBackup
    {
        public IScene Scene;
        public EntityReference? Camera;
    }
}
