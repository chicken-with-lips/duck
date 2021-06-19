using System.Numerics;
using Duck.Contracts;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;

namespace Duck.FilamentBridge.Systems
{
    public class SyncRenderTransformsSystem : SystemBase
    {
        private IFilter<TransformComponent, FilamentIdentityComponent> _filter;
        private RenderingSubsystem _renderingSubsystem;
        private TransformManager _transformManager;

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _renderingSubsystem = app.GetSubsystem<RenderingSubsystem>();
            _transformManager = _renderingSubsystem.Engine.TransformManager;

            _filter = Filter<TransformComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            // _transformManager.OpenLocalTransformTransaction();

            foreach (var entity in _filter.EntityList) {
                var transformCmp = _filter.Get1(entity);
                var identityCmp = _filter.Get2(entity);

                var newTransform = Matrix4x4.CreateScale(transformCmp.Scale) * Matrix4x4.CreateFromQuaternion(transformCmp.Rotation) * Matrix4x4.CreateTranslation(transformCmp.Translation);
                // var newTransform = Matrix4x4.CreateFromQuaternion(transformCmp.Rotation) * Matrix4x4.CreateTranslation(transformCmp.Translation) * Matrix4x4.CreateScale(transformCmp.Scale);

                var instance = _transformManager.GetInstance(identityCmp.Id);
                _transformManager.SetTransform(instance, newTransform);
            }

            // _transformManager.CommitLocalTransformTransaction();
        }
    }
}
