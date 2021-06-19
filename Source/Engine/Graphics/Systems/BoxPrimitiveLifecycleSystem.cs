using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Duck.Contracts.AssetManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Rendering.Components;
using Duck.SceneManagement.Components;
using Filament;
using StbImageSharp;
using Material = Duck.AssetManagement.Material;
using MaterialInstance = Duck.AssetManagement.MaterialInstance;

namespace Duck.Rendering.Systems
{
    public class BoxPrimitiveLifecycleSystem : SystemBase
    {
        private readonly IFilter<BoxPrimitiveComponent, TransformComponent, FilamentIdentityComponent> _filter;

        private readonly Engine _engine;
        private readonly IAssetSubsystem _assetSubsystem;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly Material _material;
        private readonly MaterialInstance _materialInstance;

        public BoxPrimitiveLifecycleSystem(IWorld world, Engine engine, IAssetSubsystem assetSubsystem)
        {
            _engine = engine;
            _assetSubsystem = assetSubsystem;
            _vertexBuffer = InitVertexBuffer();
            _indexBuffer = InitIndexBuffer();

            // _material = CreateMaterial();
            // _materialInstance = CreateMaterialInstance(_material);

            _filter = Filter<BoxPrimitiveComponent, TransformComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entityId in _filter.EntityAddedList) {
                TransformComponent transformComponent = _filter.Get2(entityId);
                FilamentIdentityComponent identityComponent = _filter.Get3(entityId);

                Box boundingBox = new Box(
                    new Vector3(0, 0, 0),
                    new Vector3(0.5f, 0.5f, 0.5f)
                );

                MaterialInstance materialInstance = (MaterialInstance) _assetSubsystem.LoadMaterialInstanceImmediate(
                    _assetSubsystem.GetReference<IMaterialInstanceAsset>(new Uri("memory:///PolygonPrototype_Global_Grid_06")),
                    new Vector2(transformComponent.Scale.X, transformComponent.Scale.Z)
                    // new Vector2(1, 1)
                );


                RenderableBuilder.Create()
                    .WithBoundingBox(boundingBox)
                    .WithGeometry(0, PrimitiveType.Triangles, _vertexBuffer, _indexBuffer, 0, 36)
                    .WithMaterial(0, materialInstance.InternalMaterialInstance)
                    .Build(_engine, identityComponent.Id);

                ref BoundingBoxComponent boundingBoxCmp = ref _filter.GetEntity(entityId).Get<BoundingBoxComponent>();
                boundingBoxCmp.Box = boundingBox;
            }
        }

        private VertexBuffer InitVertexBuffer()
        {
            // Create tangent frames, one per face
            var tfPX = new float[4];
            var tfNX = new float[4];
            var tfPY = new float[4];
            var tfNY = new float[4];
            var tfPZ = new float[4];
            var tfNZ = new float[4];

            MathUtils.PackTangentFrame(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, 0.0f), tfPX);
            MathUtils.PackTangentFrame(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -0.5f), new Vector3(-0.5f, 0.0f, 0.0f), tfNX);
            MathUtils.PackTangentFrame(new Vector3(-0.5f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -0.5f), new Vector3(0.0f, 0.5f, 0.0f), tfPY);
            MathUtils.PackTangentFrame(new Vector3(-0.5f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.5f), new Vector3(0.0f, -0.5f, 0.0f), tfNY);
            MathUtils.PackTangentFrame(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.5f), tfPZ);
            MathUtils.PackTangentFrame(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -0.5f), tfNZ);

            var vbo = new VertexBufferObject();
            vbo.Write(new Vector3(-0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));
            // Face +X
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));
            // Face +Z
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));
            // Face -X
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(-0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));
            // Face -Y
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(-0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));
            // Face +Y
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector2(0.0f, 0.0f));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector2(1.0f, 0.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector2(1.0f, 1.0f));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector2(0.0f, 1.0f));

            // // Front face
            // vbo.Write(new Vector3(-1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(-1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            // vbo.Write(new Vector2(0.0f,  1.0f));
            //
            // // Back face
            // vbo.Write(new Vector3(-1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(-1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            // vbo.Write(new Vector2(0.0f,  1.0f));

            // // Top face
            // vbo.Write(new Vector3(-1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(-1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            // vbo.Write(new Vector2(0.0f,  1.0f));
            //
            // // Bottom face
            // vbo.Write(new Vector3(-1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(-1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector2(0.0f,  1.0f));
            //
            // // Right face
            // vbo.Write(new Vector3(1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector2(0.0f,  1.0f));
            //
            // // Left face
            // vbo.Write(new Vector3(-1.0f, -1.0f, -1.0f));
            // vbo.Write(new Vector2(0.0f,  0.0f));
            // vbo.Write(new Vector3(-1.0f, -1.0f, 1.0f));
            // vbo.Write(new Vector2(1.0f,  0.0f));
            // vbo.Write(new Vector3(-1.0f, 1.0f, 1.0f));
            // vbo.Write(new Vector2(1.0f,  1.0f));
            // vbo.Write(new Vector3(-1.0f, 1.0f, -1.0f));
            // vbo.Write(new Vector2(0.0f,  1.0f));

            var vertexBuffer = VertexBufferBuilder.Create()
                .WithVertexCount(24)
                .WithBufferCount(1)
                .WithAttribute(VertexAttribute.Position, 0, ElementType.Float3, 0, 36)
                .WithAttribute(VertexAttribute.Tangents, 0, ElementType.Float4, 12, 36)
                .WithAttribute(VertexAttribute.Uv0, 0, ElementType.Float2, 28, 36)
                .Build(_engine);
            vertexBuffer.SetBufferAt(_engine, 0, vbo);

            return vertexBuffer;
        }

        private IndexBuffer InitIndexBuffer()
        {
            List<ushort> indices = new();

            for (var it = 0; it < 6; it++) {
                var i = (ushort) (it * 4);

                indices.Add(i);
                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));
                indices.Add((ushort) i);
                indices.Add((ushort) (i + 2));
                indices.Add((ushort) (i + 3));
            }

            var indexBuffer = IndexBufferBuilder.Create()
                .WithIndexCount(indices.Count)
                .WithBufferType(IndexType.UShort)
                .Build(_engine);
            indexBuffer.SetBuffer(_engine, indices.ToArray());

            return indexBuffer;
        }
    }
}
