using Duck.Contracts;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering;
using Duck.Rendering.Components;
using Duck.SceneManagement;
using Duck.SceneManagement.Components;
using Filament.CameraUtilities;

namespace Duck.Renderering.Systems
{
    public class CameraLifecycleSystem : SystemBase
    {
        private IFilter<CameraComponent, FilamentIdentityComponent>? _filter;
        private RenderingSubsystem? _renderingSubsystem;
        private Scene? _scene;

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _renderingSubsystem = app.GetSubsystem<RenderingSubsystem>();
            _scene = scene as Scene;

            _filter = Filter<CameraComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entity in _filter.EntityAddedList) {
                var identity = _filter.Get2(entity);
                var camera = _renderingSubsystem?.Engine.CreateCamera(identity.Id);

                // FIXME: aspect needs updated on resize events
                var width = (float) _renderingSubsystem.NativeWindow.Width;
                var height = (float) _renderingSubsystem.NativeWindow.Height;
                var aspect = width / height;

                camera.SetProjection(45.0f, aspect, 0.1f, 2000.0f, FieldOfView.Vertical);
                camera.SetExposure(16.0f, 1.0f / 125.0f, 100.0f);

                _scene.RenderingView.Camera = camera;
            }
        }
    }
}
