namespace Duck.Content;

public interface ITexture2DAsset : IAsset
{
    public int AnisotropyLevel { get; }
    public MinFilter MinFilter { get; }
    public MagFilter MagFilter { get; }
    public WrapMode WrapS { get; }
    public WrapMode WrapR { get; }
    public WrapMode WrapT { get; }
}

public interface ITexture2D
{
    public ITexture2DAsset Asset { get; }
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
