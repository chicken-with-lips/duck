namespace Duck.Graphics.Systems;

// public class SyncRenderTransformsSystem : SystemBase
// {
//     private readonly IFilter<TransformComponent, FilamentIdentityComponent> _filter;
//     private readonly TransformManager _transformManager;
//
//     public SyncRenderTransformsSystem(IWorld world, TransformManager transformManager)
//     {
//         _transformManager = transformManager;
//
//         _filter = Filter<TransformComponent, FilamentIdentityComponent>(world)
//             .Build();
//     }
//
//     public override void Run()
//     {
//         _transformManager.OpenLocalTransformTransaction();
//
//         foreach (var entityId in _filter.EntityList) {
//             var transformComponent = _filter.Get1(entityId);
//             var identityComponent = _filter.Get2(entityId);
//
//             var instance = _transformManager.GetInstance(identityComponent.Id);
//
//             var newTransform = Matrix4x4.CreateScale(transformComponent.Scale)
//                                * Matrix4x4.CreateFromQuaternion(transformComponent.Rotation)
//                                * Matrix4x4.CreateTranslation(transformComponent.Translation);
//
//             _transformManager.SetTransform(instance, newTransform);
//         }
//
//         _transformManager.CommitLocalTransformTransaction();
//     }
// }
