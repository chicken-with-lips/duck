using Duck.Content;

namespace Duck.Graphics.Textures;

public class Texture2D : AssetBase<Texture2D>
{

    #region Properties

    public int Width { get; }
    public int Height { get; }
    public bool UseRawData { get; }
    public int AnisotropyLevel { get; set; }
    public MinFilter MinFilter { get; set; }
    public MagFilter MagFilter { get; set; }
    public WrapMode WrapS { get; set; }
    public WrapMode WrapT { get; set; }
    public WrapMode WrapR { get; set; }
    public Channels Channels { get; set; }

    #endregion

    #region Methods

    public Texture2D(AssetImportData importData, int width, int height, Channels channels = Channels.Rgba, bool useRawData = false)
        : base(importData)
    {
        Width = width;
        Height = height;
        UseRawData = useRawData;
        AnisotropyLevel = 1;
        MinFilter = MinFilter.Nearest;
        MagFilter = MagFilter.Linear;
        WrapS = WrapMode.Repeat;
        WrapT = WrapMode.Repeat;
        WrapR = WrapMode.Repeat;
        Channels = channels;
    }

    #endregion
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
}

public enum Channels
{
    Rgb,
    Rgba,
}
