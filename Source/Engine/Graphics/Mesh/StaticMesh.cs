using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;

namespace Duck.Graphics.Mesh;

public class StaticMesh : AssetBase<StaticMesh>
{
    public readonly AssetReference<ShaderProgram> ShaderProgram;
    public readonly AssetReference<Texture2D> Texture;
    public readonly BufferObject<float> VertexBuffer;
    public readonly BufferObject<uint> IndexBuffer;

    public StaticMesh(
        AssetImportData importData,
        BufferObject<float> vertexBuffer,
        BufferObject<uint> indexBuffer,
        AssetReference<ShaderProgram> shaderProgram,
        AssetReference<Texture2D> texture)
        : base(importData)
    {
        ShaderProgram = shaderProgram;
        Texture = texture;
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
    }
}
