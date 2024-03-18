using System.Diagnostics;
using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Materials;
using Duck.Graphics.Mesh;

namespace Duck.RenderSystems.OpenGL.ContentLoader;

internal class StaticMeshLoader : IAssetLoader
{
    private readonly OpenGLGraphicsDevice _graphicsDevice;
    private readonly IContentModule _contentModule;

    public StaticMeshLoader(
        OpenGLGraphicsDevice graphicsDevice,
        IContentModule contentModule)
    {
        _graphicsDevice = graphicsDevice;
        _contentModule = contentModule;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is StaticMesh;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not StaticMesh meshAsset) {
            throw new Exception("FIXME: errors");
        }

        // TODO: vertex type should come from the asset

        var vertexBuffer = VertexBufferBuilder<TexturedVertex>.Create(BufferUsage.Static)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Normal, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.TexCoord0, 0, AttributeType.Float2)
            .Build(_graphicsDevice);
        vertexBuffer.SetData(0, meshAsset.VertexBuffer);

        var indexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Static)
            .Build(_graphicsDevice);
        indexBuffer.SetData(0, meshAsset.IndexBuffer);

        var material = (IPlatformAsset<Material>)_contentModule.LoadImmediate(meshAsset.Material);

        var renderObject = _graphicsDevice.CreateRenderObject(
            vertexBuffer,
            indexBuffer
        );
        renderObject.SetMaterial(material);

        foreach (var tuple in meshAsset.GetTextures()) {
            var texture = (OpenGLTexture2D)_contentModule.LoadImmediate(tuple.Texture);
            renderObject.SetTexture(tuple.Slot, texture);
        }

        if (loadInto != null) {
            Console.WriteLine("FIXME: FREE OLD DATA");

            Debug.Assert(loadInto is OpenGLStaticMesh);

            ((OpenGLStaticMesh)loadInto).RenderObject = renderObject;
        }

        return new OpenGLStaticMesh(renderObject);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        throw new NotImplementedException();
    }
}
