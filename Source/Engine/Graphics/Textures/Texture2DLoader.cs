using Duck.Content;
using Duck.Graphics.OpenGL;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Duck.Graphics.Textures;

public class Texture2DLoader : IAssetLoader
{
    private readonly OpenGLGraphicsDevice _graphicsDevice;

    public Texture2DLoader(OpenGLGraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public bool CanLoad(IAsset asset)
    {
        return asset is Texture2D;
    }

    public IPlatformAsset Load(IAsset asset, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset) || asset is not Texture2D textureAsset) {
            throw new Exception("FIXME: errors");
        }

        var image = ImageResult.FromMemory(source.ToArray(), ColorComponents.RedGreenBlueAlpha);

        var api = _graphicsDevice.API;
        var textureId = api.GenTexture();
        api.BindTexture(TextureTarget.Texture2D, textureId);

        api.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ConvertWrap(textureAsset.WrapS));
        api.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ConvertWrap(textureAsset.WrapT));
        api.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, ConvertWrap(textureAsset.WrapR));

        api.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ConvertMinFilter(textureAsset.MinFilter));
        api.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ConvertMagFilter(textureAsset.MagFilter));

        api.TexImage2D<byte>(
            GLEnum.Texture2D,
            0,
            InternalFormat.Rgba,
            (uint)image.Width,
            (uint)image.Height,
            0,
            GLEnum.Rgba,
            GLEnum.UnsignedByte,
            image.Data
        );
        api.GenerateMipmap(TextureTarget.Texture2D);

        return new OpenGLTexture2D(textureId);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is OpenGLTexture2D textureAsset) {
            _graphicsDevice.API.DeleteTexture(textureAsset.TextureId);
        }
    }

    private int ConvertWrap(WrapMode mode)
    {
        return mode switch {
            WrapMode.MirroredRepeat => (int)GLEnum.MirroredRepeat,
            WrapMode.ClampToEdge => (int)GLEnum.ClampToEdge,
            _ => (int)GLEnum.Repeat,
        };
    }

    private int ConvertMinFilter(MinFilter filter)
    {
        return filter switch {
            MinFilter.Linear => (int)GLEnum.Linear,
            MinFilter.Nearest => (int)GLEnum.Nearest,
            MinFilter.LinearMipmapNearest => (int)GLEnum.LinearMipmapNearest,
            MinFilter.NearestMipmapLinear => (int)GLEnum.NearestMipmapLinear,
            MinFilter.NearestMipmapNearest => (int)GLEnum.NearestMipmapNearest,
            _ => (int)GLEnum.LinearMipmapLinear,
        };
    }

    private int ConvertMagFilter(MagFilter filter)
    {
        return filter switch {
            MagFilter.Nearest => (int)GLEnum.Nearest,
            _ => (int)GLEnum.Linear,
        };
    }
}
