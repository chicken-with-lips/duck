using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;
using Filament.CameraUtilities;
using Scene = Duck.SceneManagement.Scene;

namespace Duck.Rendering.Systems
{
    public class CameraLifecycleSystem : SystemBase
    {
        private readonly IFilter<CameraComponent, FilamentIdentityComponent> _filter;
        private readonly NativeWindow _nativeWindow;
        private readonly Engine _engine;
        private readonly Scene _scene;

        public CameraLifecycleSystem(IWorld world, Scene scene, NativeWindow nativeWindow, Engine engine)
        {
            _nativeWindow = nativeWindow;
            _engine = engine;
            _scene = scene;

            _filter = Filter<CameraComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entityId in _filter.EntityAddedList) {
                var identityComponent = _filter.Get2(entityId);
                var camera = _engine.CreateCamera(identityComponent.Id);

                // FIXME: aspect needs updated on resize events
                var width = (float) _nativeWindow.Width;
                var height = (float) _nativeWindow.Height;
                var aspect = width / height;

                camera.SetProjection(45.0f, aspect, 0.1f, 2000.0f, FieldOfView.Vertical);
                camera.SetExposure(16.0f, 1.0f / 125.0f, 100.0f);

                _scene.RenderingView.Camera = camera;
            }
        }
    }
}
