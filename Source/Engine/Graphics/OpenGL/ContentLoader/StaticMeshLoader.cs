using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Textures;

namespace Duck.Graphics.OpenGL.ContentLoader;

public class StaticMeshLoader : IAssetLoader
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

    public bool CanLoad(IAsset asset)
    {
        return asset is StaticMesh;
    }

    public IPlatformAsset Load(IAsset asset, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset) || asset is not StaticMesh meshAsset) {
            throw new Exception("FIXME: errors");
        }

        var vertexBuffer = VertexBufferBuilder<float>.Create(BufferUsage.Static)
            .Attribute(VertexAttribute.Position, 0, AttributeType.Float3)
            .Attribute(VertexAttribute.Uv0, 0, AttributeType.Float2)
            .Build(_graphicsDevice);
        vertexBuffer.SetData(0, meshAsset.VertexBuffer);

        var indexBuffer = IndexBufferBuilder<uint>.Create(BufferUsage.Static)
            .Build(_graphicsDevice);
        indexBuffer.SetData(0, meshAsset.IndexBuffer);

        var shaderProgram = (OpenGLShaderProgram)_contentModule.LoadImmediate(meshAsset.ShaderProgram);

        var renderObject = _graphicsDevice.CreateRenderObject(
            vertexBuffer,
            indexBuffer
        );
        renderObject
            .SetShaderProgram(shaderProgram);

        foreach (var tuple in meshAsset.GetTextures()) {
            var texture = (OpenGLTexture2D)_contentModule.LoadImmediate(tuple.Texture);
            renderObject.SetTexture(tuple.Slot, texture);
        }

        return new OpenGLStaticMesh(renderObject);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        throw new NotImplementedException();
    }
}
