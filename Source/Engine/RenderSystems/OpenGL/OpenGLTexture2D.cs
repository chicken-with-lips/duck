using Duck.Content;
using Duck.Renderer.Textures;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLTexture2D : PlatformAssetBase<Texture2D>
{  
    public uint TextureId { get; internal set; }

    public OpenGLTexture2D(uint textureId)
    {
        TextureId = textureId;
    }
}
