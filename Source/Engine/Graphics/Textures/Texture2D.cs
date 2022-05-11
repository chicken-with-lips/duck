using Duck.Content;

namespace Duck.Graphics.Textures;

public class Texture2D : AssetBase<Texture2D>
{
    #region Properties

    public int AnisotropyLevel { get; }
    public MinFilter MinFilter { get; }
    public MagFilter MagFilter { get; }
    public WrapMode WrapS { get; }
    public WrapMode WrapT { get; }
    public WrapMode WrapR { get; }

    #endregion

    #region Methods

    public Texture2D(AssetImportData importData)
        : base(importData)
    {
        AnisotropyLevel = 1;
        MinFilter = MinFilter.Nearest;
        MagFilter = MagFilter.Linear;
        WrapS = WrapMode.Repeat;
        WrapT = WrapMode.Repeat;
        WrapR = WrapMode.Repeat;
    }

    #endregion
}

public class OpenGLTexture2D : IPlatformAsset<Texture2D>
{  
    public uint TextureId { get; }

    public OpenGLTexture2D(uint textureId)
    {
        TextureId = textureId;
    }
}

/// <summary>
/// Sampler minification filter.
/// </summary>
public enum MinFilter : byte
{
    /// <summary>No filtering. Nearest neighbor is used.</summary>
    Nearest = 0,

    /// <summary>Box filtering. Weighted average of 4 neighbors is used.</summary>
    Linear = 1,

    /// <summary>Mip-mapping is activated. But no filtering occurs.</summary>
    NearestMipmapNearest = 2,

    /// <summary>Box filtering within a mip-map level.</summary>
    LinearMipmapNearest = 3,

    /// <summary>Mip-map levels are interpolated, but no other filtering occurs.</summary>
    NearestMipmapLinear = 4,

    /// <summary>Both interpolated Mip-mapping and linear filtering are used.</summary>
    LinearMipmapLinear = 5
}

/// <summary>
/// Sampler magnification filter
/// </summary>
public enum MagFilter : byte
{
    /// <summary>No filtering. Nearest neighbor is used.</summary>
    Nearest = 0,

    /// <summary>Box filtering. Weighted average of 4 neighbors is used.</summary>
    Linear = 1,
}

public enum WrapMode : byte
{
    /// <summary>The edge of the texture extends to infinity</summary>
    ClampToEdge,

    /// <summary>The texture infinitely repeats in the wrap direction</summary>
    Repeat,

    /// <summary>The texture infinitely repeats and mirrors in the wrap direction</summary>
    MirroredRepeat
};
