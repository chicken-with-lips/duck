using Arch.Core.Extensions;
using Duck.Content;
using Duck.Input;
using Duck.Logging;
using Duck.Renderer;
using Duck.Renderer.Components;
using Duck.Renderer.Device;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;
using Duck.Ui.Artery;
using Duck.UI.Content;
using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public class UiModule : IUiModule, IInitializableModule, IPreTickableModule, IRenderableModule
{
    #region Properties

    public Context Context => _context;

    #endregion

    #region Members

    private readonly ILogger _logger;
    private readonly IContentModule _contentModule;
    private readonly IRendererModule _rendererModule;
    private readonly IInputModule _inputModule;
    private readonly Context _context = new();

    private IPlatformAsset<Material> _coloredMaterial;
    private IPlatformAsset<Material> _texturedMaterial;
    private IPlatformAsset<Material> _textMaterial;
    private readonly RootRenderer _rootRenderer = new();

    private IRenderObject? _textRenderObject;
    private IVertexBuffer<TexturedVertex> _textVertexBuffer;
    private IIndexBuffer<uint> _textIndexBuffer;

    private IRenderObject? _boxRenderObject;
    private IVertexBuffer<TexturedVertex> _boxVertexBuffer;
    private IIndexBuffer<uint> _boxIndexBuffer;

    private readonly RenderList _renderList = new();

    #endregion

    #region Methods

    public UiModule(ILogModule logModule, IContentModule contentModule, IRendererModule rendererModule, IInputModule inputModule)
    {
        _contentModule = contentModule;
        _rendererModule = rendererModule;
        _inputModule = inputModule;

        _logger = logModule.CreateLogger("UI");
        _logger.LogInformation("Created user interface module.");
    }

    public bool Init()
    {
        _contentModule.RegisterAssetLoader<Font, ArteryFont>(new FontLoader(_contentModule));

        CreateShaders();

        _context.AddElementType<Root>(new RootFactory());
        _context.AddElementType<Window>(new WindowFactory());
        _context.AddElementType<Button>(new ButtonFactory());
        _context.AddElementType<Label>(new LabelFactory());
        _context.AddElementType<Panel>(new PanelFactory());
        _context.AddElementType<HorizontalContainer>(new HorizontalContainerFactory());
        _context.AddElementType<VerticalContainer>(new VerticalContainerFactory());

        _texturedMaterial = (IPlatformAsset<Material>)_contentModule.LoadImmediate(
            _contentModule.Database.GetAsset<Material>(new Uri("memory:///ui/textured.mat")).MakeSharedReference()
        );

        _textMaterial = (IPlatformAsset<Material>)_contentModule.LoadImmediate(
            _contentModule.Database.GetAsset<Material>(new Uri("memory:///ui/text.mat")).MakeSharedReference()
        );

        _boxVertexBuffer = VertexBufferBuilder<TexturedVertex>.Create(BufferUsage.Dynamic)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Normal, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Attribute(VertexAttribute.Color0, 0, AttributeType.Float4)
            .Build(_rendererModule.GraphicsDevice);

        _boxIndexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Dynamic)
            .Build(_rendererModule.GraphicsDevice);

        _boxRenderObject = _rendererModule.GraphicsDevice.CreateRenderObject(
            _boxVertexBuffer,
            _boxIndexBuffer
        );
        _boxRenderObject.Projection = Projection.Orthographic;
        // _boxRenderObject.RenderStateFlags = RenderStateFlag.DisableDepthTesting;
        _boxRenderObject.SetMaterial(_texturedMaterial);

        _textVertexBuffer = VertexBufferBuilder<TexturedVertex>.Create(BufferUsage.Dynamic)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Normal, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Attribute(VertexAttribute.Color0, 0, AttributeType.Float4)
            .Build(_rendererModule.GraphicsDevice);

        _textIndexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Dynamic)
            .Build(_rendererModule.GraphicsDevice);

        _textRenderObject = _rendererModule.GraphicsDevice.CreateRenderObject(
            _textVertexBuffer,
            _textIndexBuffer
        );
        _textRenderObject.Projection = Projection.Orthographic;
        _textRenderObject.RenderStateFlags = RenderStateFlag.DisableDepthTesting;
        _textRenderObject.SetMaterial(_textMaterial);

        return true;
    }

    public void PreTick()
    {
        _context.BeginFrame();
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

    private uint[] textIndices = new uint[1024];
    private uint[] boxIndices = new uint[1024];
    private TexturedVertex[] textVertices = new TexturedVertex[1024];
    private TexturedVertex[] boxVertices = new TexturedVertex[1024];

    public void Render()
    {
        if (_rendererModule.Views.Length > 1) {
            throw new Exception("Multiple views not supported");
        }

        _renderList.Clear();

        var view = _rendererModule.Views[0];

        foreach (ref var root in _context.Roots) {
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
            }, _renderList);
        }

        var primitives = _renderList.Primitives;

        if (primitives.Length == 0) {
            return;
        }

        var boxIndexCount = _renderList.BoxCount * 6;
        var boxVertexCount = _renderList.BoxCount * 4;

        uint boxIndicesIndex = 0;
        uint boxVerticesIndex = 0;

        var textIndexCount = _renderList.TextCharacterCount * 6;
        var textVertexCount = _renderList.TextCharacterCount * 4;

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
                    _textRenderObject.SetTexture(0, font.Textures[0]);

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

                        textIndices[textIndicesIndex] = textVerticesIndex;
                        textIndices[textIndicesIndex + 1] = textVerticesIndex + 3;
                        textIndices[textIndicesIndex + 2] = textVerticesIndex + 1;

                        textIndices[textIndicesIndex + 3] = textVerticesIndex + 1;
                        textIndices[textIndicesIndex + 4] = textVerticesIndex + 3;
                        textIndices[textIndicesIndex + 5] = textVerticesIndex + 2;

                        var x0 = glyph.PlaneBounds.Left * scale;
                        var x1 = glyph.PlaneBounds.Right * scale;
                        var y0 = glyph.PlaneBounds.Top * scale;
                        var y1 = glyph.PlaneBounds.Bottom * scale;

                        var textTopLeft = new Vector3D<float>(xOffset + x0, yOffset + y0, 0);
                        var textTopRight = new Vector3D<float>(xOffset + x1, yOffset + y0, 0);
                        var textBottomRight = new Vector3D<float>(xOffset + x1, yOffset + y1, 0);
                        var textBottomLeft = new Vector3D<float>(xOffset + x0, yOffset + y1, 0);

                        textVertices[textVerticesIndex] = new TexturedVertex(textTopLeft, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Left, glyph.AtlasCoordinates.Top), primitive.Color.ToVector());
                        textVertices[textVerticesIndex + 1] = new TexturedVertex(textTopRight, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Right, glyph.AtlasCoordinates.Top), primitive.Color.ToVector());
                        textVertices[textVerticesIndex + 2] = new TexturedVertex(textBottomRight, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Right, glyph.AtlasCoordinates.Bottom), primitive.Color.ToVector());
                        textVertices[textVerticesIndex + 3] = new TexturedVertex(textBottomLeft, Vector3D<float>.Zero, new Vector2D<float>(glyph.AtlasCoordinates.Left, glyph.AtlasCoordinates.Bottom), primitive.Color.ToVector());

                        xOffset += advance;

                        textIndicesIndex += 6;
                        textVerticesIndex += 4;
                    }

                    break;

                case RenderPrimitiveType.Box:
                    boxIndices[boxIndicesIndex] = boxVerticesIndex;
                    boxIndices[boxIndicesIndex + 1] = boxVerticesIndex + 3;
                    boxIndices[boxIndicesIndex + 2] = boxVerticesIndex + 1;

                    boxIndices[boxIndicesIndex + 3] = boxVerticesIndex + 1;
                    boxIndices[boxIndicesIndex + 4] = boxVerticesIndex + 3;
                    boxIndices[boxIndicesIndex + 5] = boxVerticesIndex + 2;

                    var boxTopLeft = new Vector3D<float>(positionInPixels.X, positionInPixels.Y, 0);
                    var boxTopRight = new Vector3D<float>(positionInPixels.X + dimensionsInPixels.X, positionInPixels.Y, 0);
                    var boxBottomRight = new Vector3D<float>(positionInPixels.X + dimensionsInPixels.X, positionInPixels.Y + dimensionsInPixels.Y, 0);
                    var boxBottomLeft = new Vector3D<float>(positionInPixels.X, positionInPixels.Y + dimensionsInPixels.Y, 0);

                    boxVertices[boxVerticesIndex] = new TexturedVertex(boxTopLeft, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                    boxVertices[boxVerticesIndex + 1] = new TexturedVertex(boxTopRight, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                    boxVertices[boxVerticesIndex + 2] = new TexturedVertex(boxBottomRight, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());
                    boxVertices[boxVerticesIndex + 3] = new TexturedVertex(boxBottomLeft, Vector3D<float>.Zero, Vector2D<float>.Zero, primitive.Color.ToVector());

                    boxIndicesIndex += 6;
                    boxVerticesIndex += 4;
                    break;
            }
        }

        if (boxIndexCount == 0 && textIndexCount == 0) {
            return;
        }

        if (boxIndexCount > 0) {
            _boxIndexBuffer.SetData(0, boxIndices.AsSpan(0, boxIndexCount));
            _boxVertexBuffer.SetData(0, boxVertices.AsSpan(0, boxVertexCount));
        }

        if (textIndexCount > 0) {
            _textIndexBuffer.SetData(0, textIndices.AsSpan(0, textIndexCount));
            _textVertexBuffer.SetData(0, textVertices.AsSpan(0, textVertexCount));
        }

        // foreach (var view in _rendererModule.Views) {
        var cameraRef = view.Camera;

        if (!view.IsValid || !cameraRef.HasValue) {
            return;
        }

        // FIXME: this does not support multithreading

        var cameraTransform = cameraRef.Value.Entity.Get<TransformComponent>();

        var commandBuffer = _rendererModule.GraphicsDevice.CreateCommandBuffer(view);
        commandBuffer.ViewMatrix =
            Matrix4X4.CreateLookAt(
                cameraTransform.Position,
                cameraTransform.Position + cameraTransform.Forward,
                cameraTransform.Up
            );

        if (_boxIndexBuffer.ElementCount > 0) {
            commandBuffer.ScheduleRenderable(_boxRenderObject);
        }

        if (_textIndexBuffer.ElementCount > 0) {
            commandBuffer.ScheduleRenderable(_textRenderObject);
        }
        
        _rendererModule.GraphicsDevice.Render(commandBuffer);
    }

    #endregion
}
