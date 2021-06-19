using System;
using System.IO;
using System.Numerics;
using System.Threading;
using Duck.Contracts.AssetManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;
using Filament.MeshIO;
using StbImageSharp;
using Material = Duck.AssetManagement.Material;
using MaterialInstance = Duck.AssetManagement.MaterialInstance;
using Mesh = Duck.AssetManagement.Mesh;
using Scene = Duck.SceneManagement.Scene;

namespace Duck.Rendering.Systems
{
    public class MeshLifecycleSystem : SystemBase
    {
        private readonly IFilter<MeshComponent, FilamentIdentityComponent, TransformComponent> _filter;

        private readonly Scene _scene;

        private readonly IAssetSubsystem _assetSubsystem;
        private readonly Engine _engine;
        private readonly TransformManager _transformManager;
        private readonly RenderableManager _renderableManager;

        public MeshLifecycleSystem(IWorld world, Scene scene, IAssetSubsystem assetSubsystem, Engine engine, TransformManager transformManager, RenderableManager renderableManager)
        {
            _assetSubsystem = assetSubsystem;
            _engine = engine;
            _transformManager = transformManager;
            _renderableManager = renderableManager;
            _scene = scene;

            _filter = Filter<MeshComponent, FilamentIdentityComponent, TransformComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entityId in _filter.EntityAddedList) {
                var meshComponent = _filter.Get1(entityId);
                var identityComponent = _filter.Get2(entityId);
                var transformComponent = _filter.Get3(entityId);

                var mesh = (Mesh) _assetSubsystem.LoadMeshImmediate(meshComponent.Mesh, meshComponent.Materials[0], new Vector2(transformComponent.Scale.X, transformComponent.Scale.Z));
                var internalMesh = mesh.InternalMesh;

                var meshInstance = _transformManager.GetInstance(internalMesh.Renderable);
                var parentInstance = _transformManager.GetInstance(identityComponent.Id);

                _scene.RenderingScene.AddEntity(internalMesh.Renderable);

                _transformManager.SetParent(
                    meshInstance,
                    parentInstance
                );

                Box boundingBox = _renderableManager.GetAxisAlignedBoundingBox(
                    _renderableManager.GetInstance(internalMesh.Renderable)
                );

                ref var boundingBoxCmp = ref _filter.GetEntity(entityId).Get<BoundingBoxComponent>();
                boundingBoxCmp.Box = boundingBox;
            }
        }
    }
}
