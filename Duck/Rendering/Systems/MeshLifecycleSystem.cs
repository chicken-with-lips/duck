using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Duck.Contracts;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.FilamentBridge.Systems;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;
using Filament.MeshIO;
using Scene = Duck.SceneManagement.Scene;

namespace Duck.Rendering.Systems
{
    public class MeshLifecycleSystem : SystemBase
    {
        private IFilter<MeshComponent, FilamentIdentityComponent>? _filter;

        private Scene? _scene;
        private Material? _material;
        private MaterialInstance? _materialInstance;

        private RenderingSubsystem? _renderingSubsystem;
        private TransformManager? _transformManager;
        private RenderableManager? _renderableManager;

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _renderingSubsystem = app.GetSubsystem<RenderingSubsystem>();
            _transformManager = _renderingSubsystem.Engine.TransformManager;
            _renderableManager = _renderingSubsystem.Engine.RenderableManager;
            _scene = scene as Scene;

            CreateMaterial();

            _filter = Filter<MeshComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entity in _filter.EntityAddedList) {
                var renderingIdentity = _filter.Get2(entity);

                var mesh = MeshReader.LoadFromBuffer(_renderingSubsystem.Engine, File.ReadAllBytes("Content/PolygonPrototype/StaticMeshes/SM_Buildings_Floor_1x1_01.mesh"), _materialInstance);
                var meshInstance = _transformManager.GetInstance(mesh.Renderable);
                var parentInstance = _transformManager.GetInstance(renderingIdentity.Id);

                _scene.RenderingScene.AddEntity(mesh.Renderable);

                _transformManager.SetParent(
                    meshInstance,
                    parentInstance
                );

                var boundingBox = _renderableManager.GetAxisAlignedBoundingBox(
                    _renderableManager.GetInstance(mesh.Renderable)
                );

                ref var boundingBoxCmp = ref _filter.GetEntity(entity).Get<BoundingBoxComponent>();
                boundingBoxCmp.Box = boundingBox;
            }
        }

        private void CreateMaterial()
        {
            _material = MaterialBuilder.Create()
                .WithPackage(File.ReadAllBytes("lit.filamat"))
                .Build(_renderingSubsystem.Engine);

            _materialInstance = _material.CreateInstance();
            _materialInstance.SetParameter("baseColor", RgbaType.sRgb, new Color(1.0f, 0, 0));
            _materialInstance.SetParameter("metallic", 0.0f);
            _materialInstance.SetParameter("roughness", 0.3f);
        }
    }
}
