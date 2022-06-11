using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Duck.Math;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using AttributeType = Duck.Graphics.Device.AttributeType;

namespace Duck.Graphics.OpenGL;

public class OpenGLGraphicsDevice : IGraphicsDevice
{
    #region Properties

    internal GL API => _api;

    public Matrix4X4<float> ViewMatrix { get; set; }

    #endregion

    #region Members

    private readonly GL _api;
    private readonly IGLContext _context;

    private readonly Dictionary<uint, OpenGLRenderObject> _renderObjects = new();
    private readonly Dictionary<uint, OpenGLRenderObjectInstance> _renderObjectInstances = new();
    private readonly List<IRenderObject> _frameRenderables = new();

    private OpenGLShaderProgram _debugShader;
    private IRenderObject _debugBox;
    private IRenderObject _debugSphere;

    private uint _renderObjectInstanceCounter = 0;

    #endregion

    #region Methods

    public OpenGLGraphicsDevice(IGLContext context)
    {
        _context = context;

        _api = GL.GetApi(_context);
    }

    internal void Init(OpenGLShaderProgram debugShader)
    {
        _debugShader = debugShader;
        _debugBox = CreateDebugBox();
        _debugSphere = CreateDebugSphere();
    }

    public void BeginFrame()
    {
        _frameRenderables.Clear();
        _context.MakeCurrent();

        _api.Enable(GLEnum.DepthTest);
        _api.Enable(EnableCap.PolygonOffsetFill);
        _api.PolygonOffset(1, 0);

        _api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public unsafe void Render()
    {
        // TODO: move out to render graph

        foreach (var renderable in _frameRenderables) {
            var model = renderable.HasParameter("WorldPosition") ? renderable.GetParameter<Matrix4X4<float>>("WorldPosition") : Matrix4X4<float>.Identity;

            DrawRenderable(renderable, model, (renderObject) => {
                _api.DrawElements(PrimitiveType.Triangles, renderObject.IndexCount, DrawElementsType.UnsignedInt, null);
            });

            if (renderable.BoundingVolume is BoundingBoxComponent boundingComponent) {
                DrawDebugBox(renderable, boundingComponent.Box, model);
            } else if (renderable.BoundingVolume is BoundingSphereComponent boundingSphereComponent) {
                DrawDebugSphere(renderable, boundingSphereComponent.Radius, model);
            }
        }
    }

    private unsafe void DrawRenderable(IRenderObject renderable, Matrix4X4<float> model, RenderCallback renderCallback)
    {
        if (renderable is OpenGLRenderObject glRenderObject) {
            glRenderObject.Bind();
        } else if (renderable is OpenGLRenderObjectInstance glRenderObjectInstance) {
            _renderObjects[glRenderObjectInstance.ParentId].Bind();
        }

        Matrix4X4<float> view = this.ViewMatrix;
        Matrix4X4<float> projection = Matrix4X4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75f), 1280f / 1024f, 0.1f, 10000f);

        if (renderable.GetShaderProgram() is OpenGLShaderProgram glShaderProgram) {
            _api.UseProgram(glShaderProgram.ProgramId);

            int modelLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_Model");
            _api.UniformMatrix4(modelLoc, 1, false, (float*)&model);

            int viewLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_View");
            _api.UniformMatrix4(viewLoc, 1, false, (float*)&view);

            int projLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "in_Projection");
            _api.UniformMatrix4(projLoc, 1, false, (float*)&projection);
        }

        if (renderable.GetTexture(0) is OpenGLTexture2D glTexture2D) {
            _api.BindTexture(TextureTarget.Texture2D, glTexture2D.TextureId);
        }

        renderCallback(renderable);

        _api.BindVertexArray(0);
    }

    public void EndFrame()
    {
        _context.SwapBuffers();
    }

    public IIndexBuffer<T> CreateIndexBuffer<T>(BufferUsage usage)
        where T : unmanaged
    {
        return new OpenGLIndexBuffer<T>(this, _api, OpenGLUtil.Convert(usage));
    }

    public IVertexBuffer<T> CreateVertexBuffer<T>(BufferUsage usage, AttributeDecl[] attributes)
        where T : unmanaged
    {
        return new OpenGLVertexBuffer<T>(this, _api, OpenGLUtil.Convert(usage), attributes);
    }

    public IRenderObject CreateRenderObject<TDataType, TIndexType>(IVertexBuffer<TDataType> vertexBuffer, IIndexBuffer<TIndexType> indexBuffer)
        where TDataType : unmanaged
        where TIndexType : unmanaged
    {
        var obj = new OpenGLRenderObject(this, vertexBuffer, indexBuffer);
        _renderObjects.Add(obj.Id, obj);

        return obj;
    }

    public IRenderObjectInstance CreateRenderObjectInstance(IRenderObject renderObject)
    {
        var instance = new OpenGLRenderObjectInstance(++_renderObjectInstanceCounter, renderObject);
        _renderObjectInstances.Add(instance.Id, instance);

        return instance;
    }

    public void DestroyRenderObject(IRenderObject renderObject)
    {
        if (renderObject is IRenderObjectInstance) {
            _renderObjectInstances.Remove(renderObject.Id);
        } else {
            _renderObjects.Remove(renderObject.Id);
        }

        if (!renderObject.IsDisposed) {
            renderObject.Dispose();
        }
    }

    public void ScheduleRenderable(IRenderObjectInstance renderObjectInstance)
    {
        _frameRenderables.Add(renderObjectInstance);
    }

    public void ScheduleRenderable(IRenderObject renderObject)
    {
        _frameRenderables.Add(renderObject);
    }

    public void ScheduleRenderableInstance(uint instanceId)
    {
        _frameRenderables.Add(_renderObjectInstances[instanceId]);
    }

    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId)
    {
        return _renderObjectInstances[instanceComponentId];
    }

    private unsafe void DrawDebugBox(IRenderObject parentRenderObject, Box3D<float> box, Matrix4X4<float> model)
    {
        model = Matrix4X4.CreateScale(box.Size) * model;

        DrawRenderable(_debugBox, model, (renderObject) => {
            _api.DrawElements(PrimitiveType.LineLoop, 4, DrawElementsType.UnsignedShort, null);
            _api.DrawElements(PrimitiveType.LineLoop, 4, DrawElementsType.UnsignedShort, (void*)(4 * sizeof(ushort)));
            _api.DrawElements(PrimitiveType.Lines, 8, DrawElementsType.UnsignedShort, (void*)(8 * sizeof(ushort)));
        });
    }

    private unsafe void DrawDebugSphere(IRenderObject parentRenderObject, float radius, Matrix4X4<float> model)
    {
        model = Matrix4X4.CreateScale(radius, radius, radius) * model;

        DrawRenderable(_debugSphere, model, (renderObject) => {
            _api.DrawElements(PrimitiveType.Lines, _debugSphere.IndexCount, DrawElementsType.UnsignedShort, null);
        });
    }

    private IRenderObject CreateDebugBox()
    {
        var vertices = new BufferObject<DebugVertex>(new DebugVertex[] {
            new(new Vector3D<float>(-0.5f, -0.5f, -0.5f)),
            new(new Vector3D<float>(0.5f, -0.5f, -0.5f)),
            new(new Vector3D<float>(0.5f, 0.5f, -0.5f)),
            new(new Vector3D<float>(-0.5f, 0.5f, -0.5f)),
            new(new Vector3D<float>(-0.5f, -0.5f, 0.5f)),
            new(new Vector3D<float>(0.5f, -0.5f, 0.5f)),
            new(new Vector3D<float>(0.5f, 0.5f, 0.5f)),
            new(new Vector3D<float>(-0.5f, 0.5f, 0.5f)),
        });

        var indexes = new BufferObject<ushort>(new ushort[] {
            0, 1, 2, 3,
            4, 5, 6, 7,
            0, 4, 1, 5, 2, 6, 3, 7
        });

        var vertexBuffer = VertexBufferBuilder<DebugVertex>.Create(BufferUsage.Static)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Build(this);
        vertexBuffer.SetData(0, vertices);

        var indexBuffer = IndexBufferBuilder<ushort>.Create(BufferUsage.Static)
            .Build(this);
        indexBuffer.SetData(0, indexes);

        var renderObject = CreateRenderObject(
            vertexBuffer,
            indexBuffer
        );

        renderObject.SetShaderProgram(_debugShader);

        return renderObject;
    }

    private IRenderObject CreateDebugSphere()
    {
        // http://www.songho.ca/opengl/gl_sphere.html

        float PI = MathF.Acos(-1);

        var tmpVertices = new List<Vector3D<float>>();
        var tmpVertexes = new List<DebugVertex>();
        var tmpIndices = new List<ushort>();

        var radius = 1f;
        var sectorCount = 20;
        var stackCount = 10;
        float sectorStep = 2f * PI / sectorCount;
        float stackStep = PI / stackCount;
        float sectorAngle, stackAngle;

        for (int i = 0; i <= stackCount; ++i) {
            stackAngle = PI / 2 - i * stackStep;
            float xy = radius * MathF.Cos(stackAngle);
            float z = radius * MathF.Sin(stackAngle);

            for (int j = 0; j <= sectorCount; ++j) {
                sectorAngle = j * sectorStep;

                tmpVertices.Add(
                    new Vector3D<float>(
                        xy * MathF.Cos(sectorAngle),
                        xy * MathF.Sin(sectorAngle),
                        z
                    )
                );
            }
        }

        Vector3D<float> v1, v2, v3, v4;

        int k, vi1, vi2;
        ushort index = 0;
        for (int i = 0; i < stackCount; ++i) {
            vi1 = i * (sectorCount + 1);
            vi2 = (i + 1) * (sectorCount + 1);

            for (int j = 0; j < sectorCount; ++j, ++vi1, ++vi2) {
                v1 = tmpVertices[vi1];
                v2 = tmpVertices[vi2];
                v3 = tmpVertices[vi1 + 1];
                v4 = tmpVertices[vi2 + 1];

                if (i == 0) {
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v1.X, v1.Y, v1.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v2.X, v2.Y, v2.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v4.X, v4.Y, v4.Z)));

                    tmpIndices.Add(index);
                    tmpIndices.Add((ushort)(index + 1));

                    index += 3;
                } else if (i == (stackCount - 1)) {
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v1.X, v1.Y, v1.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v2.X, v2.Y, v2.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v3.X, v3.Y, v3.Z)));

                    tmpIndices.Add(index);
                    tmpIndices.Add((ushort)(index + 1));
                    tmpIndices.Add(index);
                    tmpIndices.Add((ushort)(index + 2));

                    index += 3;
                } else {
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v1.X, v1.Y, v1.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v2.X, v2.Y, v2.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v3.X, v3.Y, v3.Z)));
                    tmpVertexes.Add(new DebugVertex(new Vector3D<float>(v4.X, v4.Y, v4.Z)));

                    tmpIndices.Add(index);
                    tmpIndices.Add((ushort)(index + 1));
                    tmpIndices.Add(index);
                    tmpIndices.Add((ushort)(index + 2));

                    index += 4;
                }
            }
        }

        var vertices = new BufferObject<DebugVertex>(tmpVertexes.ToArray());
        var indexes = new BufferObject<ushort>(tmpIndices.ToArray());

        var vertexBuffer = VertexBufferBuilder<DebugVertex>.Create(BufferUsage.Static)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Build(this);
        vertexBuffer.SetData(0, vertices);

        var indexBuffer = IndexBufferBuilder<ushort>.Create(BufferUsage.Static)
            .Build(this);
        indexBuffer.SetData(0, indexes);

        var renderObject = CreateRenderObject(
            vertexBuffer,
            indexBuffer
        );

        renderObject.SetShaderProgram(_debugShader);

        return renderObject;
    }

    #endregion

    private delegate void RenderCallback(IRenderObject renderObject);
}
