using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Materials;
using Duck.Graphics.Shaders;
using Duck.Input;
using Duck.Logging;
using Duck.Platform;
using Duck.Ui.Artery;
using Duck.UI.Content;
using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public class UIModule : IUiModule, IInitializableModule, IPreTickableModule, IRenderableModule
{
    #region Members

    private readonly ILogger _logger;
    private readonly IContentModule _contentModule;
    private readonly IRendererModule _rendererModule;
    private readonly IInputModule _inputModule;

    private IPlatformAsset<Material> _coloredMaterial;
    private IPlatformAsset<Material> _texturedMaterial;
    private IPlatformAsset<Material> _textMaterial;

    private readonly RootRenderer _rootRenderer = new();
    private readonly Dictionary<IScene, ContextData> _contexts = new();

    #endregion

    #region Methods

    public UIModule(ILogModule logModule, IContentModule contentModule, IRendererModule rendererModule, IInputModule inputModule)
    {
        _contentModule = contentModule;
        _rendererModule = rendererModule;
        _inputModule = inputModule;

        _logger = logModule.CreateLogger("UI");
        _logger.LogInformation("Created user interface module.");
    }

    public bool Init()
    {
        CreateShaders();

        _contentModule.RegisterAssetLoader<Font, ArteryFont>(new FontLoader(_contentModule));

        _texturedMaterial = (IPlatformAsset<Material>)_contentModule.LoadImmediate(
            _contentModule.Database.GetAsset<Material>(new Uri("memory:///ui/textured.mat")).MakeSharedReference()
        );

        _textMaterial = (IPlatformAsset<Material>)_contentModule.LoadImmediate(
            _contentModule.Database.GetAsset<Material>(new Uri("memory:///ui/text.mat")).MakeSharedReference()
        );

        return true;
    }

    public void PreTick()
    {
        foreach (var kvp in _contexts) {
            kvp.Value.Context.BeginFrame();
        }
    }

    private void CreateShaders()
    {
        var msdfFragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/text-msdf.frag"))));
        var coloredFragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui-colored.frag"))));
        var texturedFragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui-textured.frag"))));
        var vertShader = _contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui.vert"))));

        var coloredShader = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://default-ui-color.shader")),
                vertShader.MakeSharedReference(),
                coloredFragShader.MakeSharedReference()
            )
        );

        var texturedShader = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://default-ui-textured.shader")),
                vertShader.MakeSharedReference(),
                texturedFragShader.MakeSharedReference()
            )
        );

        var textShader = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://default-ui-text.shader")),
                vertShader.MakeSharedReference(),
                msdfFragShader.MakeSharedReference()
            )
        );

        var mat = new Material(
            new AssetImportData(new Uri("memory:///ui/colored.mat"))
        );
        mat.Shader = coloredShader.MakeSharedReference();

        _contentModule.Database.Register(mat);

        mat = new Material(
            new AssetImportData(new Uri("memory:///ui/textured.mat"))
        );
        mat.Shader = texturedShader.MakeSharedReference();

        _contentModule.Database.Register(mat);

        mat = new Material(
            new AssetImportData(new Uri("memory:///ui/text.mat"))
        );
        mat.Shader = textShader.MakeSharedReference();

        _contentModule.Database.Register(mat);
    }

    public void Render()
    {
        foreach (var kvp in _contexts) {
            var pair = kvp.Value;
            var renderList = pair.RenderList;
            var renderData = pair.RenderData;
            var context = pair.Context;

            var view = _rendererModule.FindViewForScene(kvp.Key);

            if (view == null) {
                continue;
            }

            renderList.Clear();

            foreach (ref var root in context.Roots) {
                if (!root.Props.Font.HasValue) {
                    throw new Exception("FIXME: font should be set");
                }

                var widthInEm = Measure.EmSizeFromPixels(view.Dimensions.X);
                var heightInEm = Measure.EmSizeFromPixels(view.Dimensions.Y);

                _rootRenderer.Render(ref root, new ElementRenderContext() {
                    ParentBox = Box.Default with {
                        ContentWidth = widthInEm,
                        ContentHeight = heightInEm,
                    },
                    ParentBoxInPixels = Box.Default with {
                        ContentWidth = view.Dimensions.X,
                        ContentHeight = view.Dimensions.Y
                    },
                    Input = _inputModule,
                }, renderList);
            }

            var primitives = renderList.Primitives;

            if (primitives.Length == 0) {
                return;
            }

            var boxIndexCount = renderList.BoxCount * 6;
            var boxVertexCount = renderList.BoxCount * 4;

            uint boxIndicesIndex = 0;
            uint boxVerticesIndex = 0;

            var textIndexCount = renderList.TextCharacterCount * 6;
            var textVertexCount = renderList.TextCharacterCount * 4;

            uint textIndicesIndex = 0;
            uint textVerticesIndex = 0;

            foreach (var primitive in primitives) {
                var positionInPixels = Measure.EmToPixels(primitive.Position);
                var dimensionsInPixels = Measure.EmToPixels(primitive.Dimensions);

                switch (primitive.Type) {
                    case RenderPrimitiveType.Text:
                        if (!primitive.Font.HasValue || string.IsNullOrEmpty(primitive.Text)) {
                            continue;
                        }

                        var font = (ArteryFont)_contentModule.LoadImmediate(primitive.Font.Value);

                        // FIXME: only one shader
                        renderData.TextRenderObject.SetTexture(0, font.Textures[0]);

                        var scale = 0.5f;
                        var xOffset = positionInPixels.X;
                        var yOffset = positionInPixels.Y + (font.Ascender * scale);

                        for (var i = 0; i < primitive.Text.Length; i++) {
                            var glyph = font.GetGlyph(primitive.Text[i]);
                            var advance = glyph.Advance.X * scale;

                            if (glyph.IsWhitespace) {
                                xOffset += advance;
                                continue;
                            }

                            renderData.TextIndices[textIndicesIndex] = textVerticesIndex;
                            renderData.TextIndices[textIndicesIndex + 1] = textVerticesIndex + 3;
                            renderData.TextIndices[textIndicesIndex + 2] = textVerticesIndex + 1;

                            renderData.TextIndices[textIndicesIndex + 3] = textVerticesIndex + 1;
                            renderData.TextIndices[textIndicesIndex + 4] = textVerticesIndex + 3;
                            renderData.TextIndices[textIndicesIndex + 5] = textVerticesIndex + 2;

                            var x0 = glyph.PlaneBounds.Left * scale;
                            var x1 = glyph.PlaneBounds.Right * scale;
                            var y0 = glyph.PlaneBounds.Top * scale;
                            var y1 = glyph.PlaneBounds.Bottom * scale;

                            var textTopLeft = new Vector3D<float>(xOffset + x0, yOffset + y0, 0);
                            var textTopRight = new Vector3D<float>(xOffset + x1, yOffset + y0, 0);
                            var textBottomRight = new Vector3D<float>(xOffset + x1, yOffset + y1, 0);
                            var textBottomLeft = new Vector3D<float>(xOffset + x0, yOffset + y1, 0);

                            renderData.TextVertices[textVerticesIndex] = new TexturedVertex(textTopLeft, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Left, glyph.AtlasCoordinates.Top), primitive.Color.ToVector());
                            renderData.TextVertices[textVerticesIndex + 1] = new TexturedVertex(textTopRight, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Right, glyph.AtlasCoordinates.Top), primitive.Color.ToVector());
                            renderData.TextVertices[textVerticesIndex + 2] = new TexturedVertex(textBottomRight, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Right, glyph.AtlasCoordinates.Bottom), primitive.Color.ToVector());
                            renderData.TextVertices[textVerticesIndex + 3] = new TexturedVertex(textBottomLeft, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Left, glyph.AtlasCoordinates.Bottom), primitive.Color.ToVector());

                            xOffset += advance;

                            textIndicesIndex += 6;
                            textVerticesIndex += 4;
                        }

                        break;

                    case RenderPrimitiveType.Box:
                        renderData.BoxIndices[boxIndicesIndex] = boxVerticesIndex;
                        renderData.BoxIndices[boxIndicesIndex + 1] = boxVerticesIndex + 3;
                        renderData.BoxIndices[boxIndicesIndex + 2] = boxVerticesIndex + 1;

                        renderData.BoxIndices[boxIndicesIndex + 3] = boxVerticesIndex + 1;
                        renderData.BoxIndices[boxIndicesIndex + 4] = boxVerticesIndex + 3;
                        renderData.BoxIndices[boxIndicesIndex + 5] = boxVerticesIndex + 2;

                        var boxTopLeft = new Vector3D<float>(positionInPixels.X, positionInPixels.Y, 0);
                        var boxTopRight = new Vector3D<float>(positionInPixels.X + dimensionsInPixels.X, positionInPixels.Y, 0);
                        var boxBottomRight = new Vector3D<float>(positionInPixels.X + dimensionsInPixels.X, positionInPixels.Y + dimensionsInPixels.Y, 0);
                        var boxBottomLeft = new Vector3D<float>(positionInPixels.X, positionInPixels.Y + dimensionsInPixels.Y, 0);

                        renderData.BoxVertices[boxVerticesIndex] = new TexturedVertex(boxTopLeft, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                        renderData.BoxVertices[boxVerticesIndex + 1] = new TexturedVertex(boxTopRight, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                        renderData.BoxVertices[boxVerticesIndex + 2] = new TexturedVertex(boxBottomRight, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                        renderData.BoxVertices[boxVerticesIndex + 3] = new TexturedVertex(boxBottomLeft, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());

                        boxIndicesIndex += 6;
                        boxVerticesIndex += 4;
                        break;
                }
            }

            if (boxIndexCount == 0 && textIndexCount == 0) {
                return;
            }

            if (boxIndexCount > 0) {
                renderData.BoxIndexBuffer.SetData(0, renderData.BoxIndices.AsSpan(0, boxIndexCount));
                renderData.BoxVertexBuffer.SetData(0, renderData.BoxVertices.AsSpan(0, boxVertexCount));
            }

            if (textIndexCount > 0) {
                renderData.TextIndexBuffer.SetData(0, renderData.TextIndices.AsSpan(0, textIndexCount));
                renderData.TextVertexBuffer.SetData(0, renderData.TextVertices.AsSpan(0, textVertexCount));
            }

            // foreach (var view in _rendererModule.Views) {
            var cameraRef = view.Camera;

            if (!view.IsValid || !cameraRef.HasValue) {
                return;
            }

            // FIXME: this does not support multithreading

            var cameraTransform = view.Scene.World.Get<TransformComponent>(cameraRef.Value.Entity);

            var commandBuffer = _rendererModule.GraphicsDevice.CreateCommandBuffer(view);
            commandBuffer.ViewMatrix = cameraTransform.CreateLookAtMatrix();

            if (renderData.BoxIndexBuffer.ElementCount > 0) {
                commandBuffer.ScheduleRenderable(renderData.BoxRenderObject);
            }

            if (renderData.TextIndexBuffer.ElementCount > 0) {
                commandBuffer.ScheduleRenderable(renderData.TextRenderObject);
            }

            _rendererModule.GraphicsDevice.Render(commandBuffer);
        }
    }

    public Context GetContextForScene(IScene scene)
    {
        // TODO: cleanup contexts when scene is destroyed

        if (_contexts.TryGetValue(scene, out var existing)) {
            return existing.Context;
        }

        var context = new Context();
        context.AddElementType<Root>(new RootFactory());
        context.AddElementType<Window>(new WindowFactory());
        context.AddElementType<Button>(new ButtonFactory());
        context.AddElementType<Label>(new LabelFactory());
        context.AddElementType<Panel>(new PanelFactory());
        context.AddElementType<HorizontalContainer>(new HorizontalContainerFactory());
        context.AddElementType<VerticalContainer>(new VerticalContainerFactory());
        context.AddElementType<RenderView>(new RenderViewFactory());

        var boxVertexBuffer = VertexBufferBuilder<TexturedVertex>.Create(BufferUsage.Dynamic)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Normal, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Attribute(VertexAttribute.Color0, 0, AttributeType.Float4)
            .Build(_rendererModule.GraphicsDevice);


        var boxIndexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Dynamic)
            .Build(_rendererModule.GraphicsDevice);

        var boxRenderObject = _rendererModule.GraphicsDevice.CreateRenderObject(
            boxVertexBuffer,
            boxIndexBuffer
        );
        boxRenderObject.Projection = Projection.Orthographic;
        boxRenderObject.RenderStateFlags = RenderStateFlag.DisableDepthTesting;
        boxRenderObject.SetMaterial(_texturedMaterial);

        var textVertexBuffer = VertexBufferBuilder<TexturedVertex>.Create(BufferUsage.Dynamic)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Normal, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Attribute(VertexAttribute.Color0, 0, AttributeType.Float4)
            .Build(_rendererModule.GraphicsDevice);

        var textIndexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Dynamic)
            .Build(_rendererModule.GraphicsDevice);

        var textRenderObject = _rendererModule.GraphicsDevice.CreateRenderObject(
            textVertexBuffer,
            textIndexBuffer
        );
        textRenderObject.Projection = Projection.Orthographic;
        textRenderObject.RenderStateFlags = RenderStateFlag.DisableDepthTesting;
        textRenderObject.SetMaterial(_textMaterial);

        var pair = new ContextData {
            Context = context,
            RenderList = new RenderList(),
            RenderData = new RenderData {
                TextRenderObject = textRenderObject,
                TextVertexBuffer = textVertexBuffer,
                TextIndexBuffer = textIndexBuffer,
                BoxRenderObject = boxRenderObject,
                BoxVertexBuffer = boxVertexBuffer,
                BoxIndexBuffer = boxIndexBuffer,
            }
        };

        _contexts.Add(scene, pair);

        return _contexts[scene].Context;
    }

    #endregion

    private class ContextData
    {
        public Context Context;
        public RenderList RenderList;
        public RenderData RenderData;
    }

    private class RenderData
    {
        public IRenderObject TextRenderObject;
        public IVertexBuffer<TexturedVertex> TextVertexBuffer;
        public IIndexBuffer<uint> TextIndexBuffer;

        public IRenderObject BoxRenderObject;
        public IVertexBuffer<TexturedVertex> BoxVertexBuffer;
        public IIndexBuffer<uint> BoxIndexBuffer;

        public uint[] TextIndices = new uint[1024];
        public uint[] BoxIndices = new uint[1024];
        public TexturedVertex[] TextVertices = new TexturedVertex[1024];
        public TexturedVertex[] BoxVertices = new TexturedVertex[1024];
    }
}
