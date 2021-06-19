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

namespace Duck.Rendering.Systems
{
    public class BoxPrimitiveLifecycleSystem : SystemBase
    {
        private RenderingSubsystem? _renderingSubsystem;
        private IFilter<BoxPrimitiveComponent, FilamentIdentityComponent>? _filter;

        private VertexBuffer? _vertexBuffer;
        private IndexBuffer? _indexBuffer;
        private Material? _material;
        private MaterialInstance? _materialInstance;

        public override void Init(IWorld world, IScene scene, IApplication app)
        {
            _renderingSubsystem = app.GetSubsystem<RenderingSubsystem>();
            _vertexBuffer = InitVertexBuffer();
            _indexBuffer = InitIndexBuffer();

            CreateMaterial();

            _filter = Filter<BoxPrimitiveComponent, FilamentIdentityComponent>(world)
                .Build();
        }

        public override void Run()
        {
            foreach (var entity in _filter.EntityAddedList) {
                var renderingIdentity = _filter.Get2(entity);

                var boundingBox = new Box(
                    new Vector3(0, 0, 0),
                    new Vector3(0.5f, 0.5f, 0.5f)
                );

                RenderableBuilder.Create()
                    .WithBoundingBox(boundingBox)
                    .WithGeometry(0, PrimitiveType.Triangles, _vertexBuffer, _indexBuffer, 0, 36)
                    .WithMaterial(0, _materialInstance)
                    .Build(_renderingSubsystem.Engine, renderingIdentity.Id);

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
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNZ[0], tfNZ[1], tfNZ[2], tfNZ[3]));
            // Face +X
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPX[0], tfPX[1], tfPX[2], tfPX[3]));
            // Face +Z
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPZ[0], tfPZ[1], tfPZ[2], tfPZ[3]));
            // Face -X
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            vbo.Write(new Vector3(-0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNX[0], tfNX[1], tfNX[2], tfNX[3]));
            // Face -Y
            vbo.Write(new Vector3(-0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector3(-0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector3(0.5f, -0.5f, -0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            vbo.Write(new Vector3(0.5f, -0.5f, 0.5f));
            vbo.Write(new Vector4(tfNY[0], tfNY[1], tfNY[2], tfNY[3]));
            // Face +Y
            vbo.Write(new Vector3(-0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector3(-0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, 0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));
            vbo.Write(new Vector3(0.5f, 0.5f, -0.5f));
            vbo.Write(new Vector4(tfPY[0], tfPY[1], tfPY[2], tfPY[3]));

            var vertexBuffer = VertexBufferBuilder.Create()
                .WithVertexCount(24)
                .WithBufferCount(1)
                .WithAttribute(VertexAttribute.Position, 0, ElementType.Float3, 0, 28)
                .WithAttribute(VertexAttribute.Tangents, 0, ElementType.Float4, 12, 28)
                .Build(_renderingSubsystem.Engine);
            vertexBuffer.SetBufferAt(_renderingSubsystem.Engine, 0, vbo);

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
                .Build(_renderingSubsystem.Engine);
            indexBuffer.SetBuffer(_renderingSubsystem.Engine, indices.ToArray());

            return indexBuffer;
        }
    }
}
