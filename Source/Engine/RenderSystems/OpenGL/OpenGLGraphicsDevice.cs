using System.Drawing;
using Duck.Renderer.Device;
using Duck.Math;
using Duck.Renderer;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using AttributeType = Duck.Renderer.Device.AttributeType;
using IWindow = Duck.Platform.IWindow;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLGraphicsDevice : IGraphicsDevice
{
    #region Properties

    internal GL API => _api;

    #endregion

    #region Members

    private readonly GL _api;
    private readonly IGLContext _context;
    private readonly IWindow _window;

    private readonly Dictionary<uint, OpenGLRenderObject> _renderObjects = new();
    private readonly Dictionary<uint, OpenGLRenderObjectInstance> _renderObjectInstances = new();

    private OpenGLMaterial? _debugMaterial;
    private IRenderObject? _debugBox;
    private IRenderObject? _debugSphere;

    private uint _renderObjectInstanceCounter = 0;

    #endregion

    #region Methods

    public OpenGLGraphicsDevice(IGLContext context, IWindow window)
    {
        _context = context;
        _window = window;

        _api = GL.GetApi(_context);
    }

    internal void Init(OpenGLMaterial debugMaterial)
    {
        _debugMaterial = debugMaterial;
        _debugBox = CreateDebugBox();
        _debugSphere = CreateDebugSphere();
    }

    public void BeginFrame()
    {
        _context.MakeCurrent();

        _api.Enable(EnableCap.PolygonOffsetFill);

        _api.PolygonOffset(1, 0);

        _api.Disable(GLEnum.CullFace);

        _api.Enable(GLEnum.StencilTest);
        _api.StencilFunc(GLEnum.Always, 1, 0);
        _api.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Keep);

        _api.Enable(GLEnum.Blend);
        _api.BlendEquation(GLEnum.FuncAdd);
        _api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        _api.ClearStencil(0);
        _api.ClearColor(Color.Black);
        _api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    public unsafe void Render(CommandBuffer commandBuffer)
    {
        // TODO: move out to render graph

        var view = commandBuffer.View;

        _api.Viewport(
            view.Position.X,
            (_window.Height - view.Position.Y) - view.Dimensions.Y,
            (uint)view.Dimensions.X,
            (uint)view.Dimensions.Y
        );

        foreach (var renderable in commandBuffer.Renderables) {
            var transform = renderable.HasParameter("WorldPosition") ? renderable.GetParameter<Matrix4X4<float>>("WorldPosition") : Matrix4X4<float>.Identity;

            DrawRenderable(renderable, commandBuffer, commandBuffer.View, commandBuffer.ViewMatrix, transform, (renderObject) => {
                _api.DrawElements(PrimitiveType.Triangles, renderObject.IndexCount, DrawElementsType.UnsignedInt, null);
            });

            // if (renderable.BoundingVolume is BoundingBoxComponent boundingComponent) {
            //     DrawDebugBox(renderable, boundingComponent.Box, transform);
            // } else if (renderable.BoundingVolume is BoundingSphereComponent boundingSphereComponent) {
            //     DrawDebugSphere(renderable, boundingSphereComponent.Radius, transform);
            // }
        }
    }

    public void EndFrame()
    {
        _context.SwapBuffers();
    }

    private unsafe void DrawRenderable(IRenderObject renderable, CommandBuffer commandBuffer, View view, Matrix4X4<float> viewMatrix, Matrix4X4<float> transform, RenderCallback renderCallback)
    {
        if (renderable is OpenGLRenderObject glRenderObject) {
            glRenderObject.Bind();
        } else if (renderable is OpenGLRenderObjectInstance glRenderObjectInstance) {
            _renderObjects[glRenderObjectInstance.ParentId].Bind();
        }

        if (renderable.RenderStateFlags.HasFlag(RenderStateFlag.DisableDepthTesting)) {
            _api.Disable(GLEnum.DepthTest);
        } else {
            _api.Enable(GLEnum.DepthTest);
        }

        Matrix4X4<float> projection = CreateProjectionMatrix(renderable, view);

        if (renderable.GetMaterial() is OpenGLMaterial glMaterial) {
            var glShaderProgram = glMaterial.ShaderProgram;

            _api.UseProgram(glShaderProgram.ProgramId);
            OpenGLUtil.LogErrors(_api);

            int modelLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "uTransform");
            OpenGLUtil.LogErrors(_api);
            _api.UniformMatrix4(modelLoc, 1, false, (float*)&transform);
            OpenGLUtil.LogErrors(_api);

            int viewLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "uView");
            OpenGLUtil.LogErrors(_api);
            _api.UniformMatrix4(viewLoc, 1, false, (float*)&viewMatrix);
            OpenGLUtil.LogErrors(_api);

            int projLoc = _api.GetUniformLocation(glShaderProgram.ProgramId, "uProjection");
            OpenGLUtil.LogErrors(_api);
            _api.UniformMatrix4(projLoc, 1, false, (float*)&projection);
            OpenGLUtil.LogErrors(_api);

            int loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "uViewPosition");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.CameraPosition.X, Time.CameraPosition.Y, Time.CameraPosition.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "material.diffuse");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, 0);
            OpenGLUtil.LogErrors(_api);

            _api.ActiveTexture(TextureUnit.Texture0);
            _api.BindTexture(TextureTarget.Texture2D, glMaterial.DiffuseTexture?.TextureId ?? 0);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "material.specular");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, glMaterial.Material.Specular.X, glMaterial.Material.Specular.Y, glMaterial.Material.Specular.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "material.shininess");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, glMaterial.Material.Shininess);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "enabledLights");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, (int) commandBuffer.DirectionalLights.Length, (int) 0, (int) 0);
            OpenGLUtil.LogErrors(_api);

            for (var i = 0; i < commandBuffer.DirectionalLights.Length; i++) {
                var directionalLight = commandBuffer.DirectionalLights[i];

                loc = _api.GetUniformLocation(glShaderProgram.ProgramId, $"directionalLights[{i}].ambient");
                OpenGLUtil.LogErrors(_api);
                _api.Uniform3(loc, Time.DirectionalLightAmbient.X, directionalLight.Ambient.Y, directionalLight.Ambient.Z);
                OpenGLUtil.LogErrors(_api);

                loc = _api.GetUniformLocation(glShaderProgram.ProgramId, $"directionalLights[{i}].diffuse");
                OpenGLUtil.LogErrors(_api);
                _api.Uniform3(loc, directionalLight.Diffuse.X, directionalLight.Diffuse.Y, directionalLight.Diffuse.Z);
                OpenGLUtil.LogErrors(_api);

                loc = _api.GetUniformLocation(glShaderProgram.ProgramId, $"directionalLights[{i}].specular");
                OpenGLUtil.LogErrors(_api);
                _api.Uniform3(loc, directionalLight.Specular.X, directionalLight.Specular.Y, directionalLight.Specular.Z);
                OpenGLUtil.LogErrors(_api);

                loc = _api.GetUniformLocation(glShaderProgram.ProgramId, $"directionalLights[{i}].direction");
                OpenGLUtil.LogErrors(_api);
                _api.Uniform3(loc, directionalLight.Direction.X, directionalLight.Direction.Y, directionalLight.Direction.Z);
                OpenGLUtil.LogErrors(_api);
            }

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.ambient");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.PointLightAmbient.X, Time.PointLightAmbient.Y, Time.PointLightAmbient.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.diffuse");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.PointLightDiffuse.X, Time.PointLightDiffuse.Y, Time.PointLightDiffuse.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.specular");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.PointLightSpecular.X, Time.PointLightSpecular.Y, Time.PointLightSpecular.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.constant");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.PointLightConstant);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.linear");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.PointLightLinear);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.quadratic");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.PointLightQuadratic);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "pointLight.position");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.PointLightPosition.X, Time.PointLightPosition.Y, Time.PointLightPosition.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.ambient");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.SpotLightAmbient.X, Time.SpotLightAmbient.Y, Time.SpotLightAmbient.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.diffuse");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.SpotLightDiffuse.X, Time.SpotLightDiffuse.Y, Time.SpotLightDiffuse.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.specular");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.SpotLightSpecular.X, Time.SpotLightSpecular.Y, Time.SpotLightSpecular.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.constant");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.SpotLightConstant);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.linear");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.SpotLightLinear);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.quadratic");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.SpotLightQuadratic);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.innerCutOff");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.SpotLightInnerCutoff);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.outerCutOff");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform1(loc, Time.SpotLightOuterCutoff);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.position");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.SpotLightPosition.X, Time.SpotLightPosition.Y, Time.SpotLightPosition.Z);
            OpenGLUtil.LogErrors(_api);

            loc = _api.GetUniformLocation(glShaderProgram.ProgramId, "spotLight.direction");
            OpenGLUtil.LogErrors(_api);
            _api.Uniform3(loc, Time.SpotLightDirection.X, Time.SpotLightDirection.Y, Time.SpotLightDirection.Z);
            OpenGLUtil.LogErrors(_api);

            if (renderable.GetTexture(0) is OpenGLTexture2D glTexture2D) {
                _api.BindTexture(TextureTarget.Texture2D, glTexture2D.TextureId);
                OpenGLUtil.LogErrors(_api);
            }
        }

        renderCallback(renderable);

        _api.BindVertexArray(0);
    }

    private Matrix4X4<float> CreateProjectionMatrix(IRenderObject renderable, View view)
    {
        if (renderable.Projection == Projection.Orthographic) {
            return Matrix4X4.CreateOrthographicOffCenter(view.Position.X, view.Dimensions.X, view.Dimensions.Y, _window.Height - view.Dimensions.Y, -10000f, 10000f);
        } else if (renderable.Projection == Projection.Perspective) {
            return Matrix4X4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75f), (float)view.Dimensions.X / (float)view.Dimensions.Y, 0.1f, 20000f);
        }

        throw new Exception("Unknown projection type");
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
        obj.Projection = Projection.Perspective;

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


    public IRenderObjectInstance GetRenderObjectInstance(uint instanceComponentId)
    {
        return _renderObjectInstances[instanceComponentId];
    }

    public CommandBuffer CreateCommandBuffer(View view)
    {
        return new CommandBuffer(view, this);
    }

    /*private unsafe void DrawDebugBox(IRenderObject parentRenderObject, Box3D<float> box, Matrix4X4<float> transform)
    {
        transform = Matrix4X4.CreateScale(box.Size) * transform;

        DrawRenderable(_debugBox, transform, (renderObject) => {
            _api.DrawElements(PrimitiveType.LineLoop, 4, DrawElementsType.UnsignedShort, null);
            _api.DrawElements(PrimitiveType.LineLoop, 4, DrawElementsType.UnsignedShort, (void*)(4 * sizeof(ushort)));
            _api.DrawElements(PrimitiveType.Lines, 8, DrawElementsType.UnsignedShort, (void*)(8 * sizeof(ushort)));
        });
    }

    private unsafe void DrawDebugSphere(IRenderObject parentRenderObject, float radius, Matrix4X4<float> transform)
    {
        transform = Matrix4X4.CreateScale(radius, radius, radius) * transform;

        DrawRenderable(_debugSphere, transform, (renderObject) => {
            _api.DrawElements(PrimitiveType.Lines, _debugSphere.IndexCount, DrawElementsType.UnsignedShort, null);
        });
    }*/

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

        renderObject.SetMaterial(_debugMaterial);

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

        renderObject.SetMaterial(_debugMaterial);

        return renderObject;
    }

    #endregion

    private delegate void RenderCallback(IRenderObject renderObject);
}
