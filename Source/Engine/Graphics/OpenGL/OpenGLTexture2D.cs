using Duck.Content;
using Duck.Graphics.Textures;

namespace Duck.Graphics.OpenGL;

public class OpenGLTexture2D : IPlatformAsset<Texture2D>
{  
    public uint TextureId { get; }

    public OpenGLTexture2D(uint textureId)
    {
        TextureId = textureId;
    }
}
