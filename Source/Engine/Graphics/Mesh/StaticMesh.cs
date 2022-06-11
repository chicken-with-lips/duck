using System.Diagnostics;
using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;

namespace Duck.Graphics.Mesh;

public class StaticMesh : AssetBase<StaticMesh>
{
    #region Properties

    public AssetReference<ShaderProgram> ShaderProgram { get; }
    public BufferObject<TexturedVertex> VertexBuffer { get; }
    public BufferObject<uint> IndexBuffer { get; }

    #endregion

    #region Members

    public Dictionary<uint, AssetReference<Texture2D>> _textures = new();

    #endregion

    public StaticMesh(
        AssetImportData importData,
        BufferObject<TexturedVertex> vertexBuffer,
        BufferObject<uint> indexBuffer,
        AssetReference<ShaderProgram> shaderProgram)
        : base(importData)
    {
        ShaderProgram = shaderProgram;
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
    }

    public StaticMesh SetTexture(uint slot, AssetReference<Texture2D> texture)
    {
        Debug.Assert(slot < IRenderObject.MaxTextureSlots);

        _textures[slot] = texture;

        return this;
    }

    public AssetReference<Texture2D> GetTexture(uint slot)
    {
        return _textures[slot];
    }

    public TextureSlot[] GetTextures()
    {
        var ret = new TextureSlot[_textures.Count];
        var index = 0;

        foreach (var (key, value) in _textures) {
            ret[index++] = new TextureSlot(key, value);
        }

        return ret;
    }
}

public readonly struct TextureSlot
{
    public readonly uint Slot;
    public readonly AssetReference<Texture2D> Texture;

    public TextureSlot(uint slot, AssetReference<Texture2D> texture)
    {
        Slot = slot;
        Texture = texture;
    }
}
