using System.Numerics;
using ChickenWithLips.RmlUi;
using Duck.Content;
using Duck.Renderer;
using Duck.Renderer.Device;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
using Silk.NET.Maths;

namespace Duck.Ui.RmlUi;

internal class RenderInterface : ChickenWithLips.RmlUi.RenderInterface
{
    #region Properties

    internal CommandBuffer? CommandBuffer { get; set; }

    #endregion

    #region Members

    private readonly IGraphicsDevice _graphicsDevice;
    private readonly IContentModule _contentModule;
    private readonly IPlatformAsset<Material> _coloredMaterial;
    private readonly IPlatformAsset<Material> _texturedMaterial;

    private readonly Dictionary<ulong, IPlatformAsset<Texture2D>> _textureLookup = new();

    #endregion

    public RenderInterface(IGraphicsDevice graphicsDevice, IContentModule contentModule, IPlatformAsset<Material> coloredMaterial, IPlatformAsset<Material> texturedMaterial)
    {
        _graphicsDevice = graphicsDevice;
        _contentModule = contentModule;
        _coloredMaterial = coloredMaterial;
        _texturedMaterial = texturedMaterial;
    }

    public override void RenderGeometry(Vertex[] vertices, int vertexCount, int[] indices, int indexCount, ulong texture, Vector2 translation)
    {
        var vertexBuffer = VertexBufferBuilder<Vertex>.Create(BufferUsage.Static)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float2)
            .Attribute(VertexAttribute.Color0, 0, AttributeType.UnsignedByte4, true)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Build(_graphicsDevice);
        vertexBuffer.SetData(0, new BufferObject<Vertex>(vertices));

        var indexBuffer = IndexBufferBuilder<int>.Create(BufferUsage.Static)
            .Build(_graphicsDevice);
        indexBuffer.SetData(0, new BufferObject<int>(indices));

        var renderObject = _graphicsDevice.CreateRenderObject(vertexBuffer, indexBuffer);
        renderObject.SetParameter("WorldPosition", Matrix4X4.CreateTranslation(translation.X, translation.Y, 0));

        if (texture > 0) {
            renderObject.SetTexture(0, _textureLookup[texture]);
            renderObject.SetMaterial(_texturedMaterial);
        } else {
            renderObject.SetMaterial(_coloredMaterial);
        }

        renderObject.Projection = Projection.Orthographic;
        renderObject.RenderStateFlags = RenderStateFlag.DisableDepthTesting;

        CommandBuffer?.ScheduleRenderable(
            renderObject
        );
    }

    public override bool GenerateTexture(out ulong textureHandle, byte[] source, int sourceSize, Vector2i sourceDimensions)
    {
        var texture = _contentModule.Database.Register(
            new Texture2D(
                new AssetImportData(
                    new Uri("memory://generated-texture")),
                sourceDimensions.X,
                sourceDimensions.Y,
                true
            )
        );

        texture.MinFilter = MinFilter.Linear;
        texture.MagFilter = MagFilter.Linear;
        texture.WrapS = WrapMode.ClampToEdge;
        texture.WrapT = WrapMode.ClampToEdge;

        // fixme: direct reference to opengl
        var loadedTexture = _contentModule.LoadImmediate(
            texture.MakeSharedReference(),
            new EmptyAssetLoadContext(),
            source
        );

        Console.WriteLine("FIXME: TEXTURE LOADING");
        // textureHandle = loadedTexture.TextureId;
        textureHandle = 0;

        // _textureLookup.Add(textureHandle, loadedTexture);

        return true;
    }

    public override bool LoadTexture(out ulong textureHandle, Vector2i textureDimensions, string source)
    {
        // FIXME: assets should be pre-registered with content database

        // var texture = _contentModule.Database.Register(
        //     new Texture2D(
        //         new AssetImportData(
        //             new Uri("file:///" + (source.StartsWith("/") ? source.Substring(1) : source))),
        //         textureDimensions.X,
        //         textureDimensions.Y
        //     )
        // );
        //
        // // fixme: direct reference to opengl
        // var glTexture = (OpenGLTexture2D)_contentModule.LoadImmediate(
        //     texture.MakeSharedReference(),
        //     new EmptyAssetLoadContext()
        // );
        //
        // textureHandle = glTexture.TextureId;
        //
        // _textureLookup.Add(textureHandle, glTexture);

        Console.WriteLine("FIXME: TEXTURE LOADING");
        textureHandle = 0;

        return true;
    }

    public override void ReleaseTexture(IntPtr textureHandle)
    {
        Console.WriteLine("TODO: RELEASE TEXTURE");

        base.ReleaseTexture(textureHandle);
    }
}
