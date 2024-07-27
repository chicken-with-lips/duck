using System.Runtime.CompilerServices;
using ChickenWithLips.ArteryFont;
using Duck.Content;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Ui.Artery;

internal class ArteryFont : PlatformAssetBase<Font>
{
    public IPlatformAsset<Texture2D>[] Textures { get; }
    public float Ascender { get; init; }
    public float Descender { get; init; }

    private readonly Dictionary<uint, Glyph> _glyphs = new();
    private readonly float _internalEmSize;
    private readonly float _internalFontSize;

    public ArteryFont(Font<float> font, in IPlatformAsset<Texture2D>[] textures)
    {
        Textures = textures;

        // FIXME: only supports single variant and image
        var image = font.Images[0];
        var variant = font.Variants[0];

        _internalFontSize = variant.Metrics.FontSize;
        _internalEmSize = variant.Metrics.EmSize;
        Ascender = variant.Metrics.Ascender * variant.Metrics.FontSize;
        Descender = variant.Metrics.Descender * variant.Metrics.FontSize;

        if (System.Math.Abs(_internalEmSize - 1f) > float.Epsilon || System.Math.Abs(_internalFontSize - 32) > float.Epsilon) {
            throw new Exception("FIXME: Em=1 FontSize=32");
        }

        for (var i = 0; i < variant.Glyphs.Length; i++) {
            var glyph = variant.Glyphs[i];

            _glyphs[glyph.Codepoint] = new Glyph() {
                Dimensions = new Vector2D<float>(
                    glyph.ImageBounds.Right - glyph.ImageBounds.Left,
                    glyph.ImageBounds.Top - glyph.ImageBounds.Bottom
                ),
                PlaneBounds = new Graphics.Rectangle<float>(
                    glyph.PlaneBounds.Left,
                    -glyph.PlaneBounds.Top,
                    glyph.PlaneBounds.Right,
                    -glyph.PlaneBounds.Bottom
                ).Scale(variant.Metrics.FontSize),
                AtlasCoordinates = new Graphics.Rectangle<float>(
                    glyph.ImageBounds.Left / image.Width,
                    (image.Height - glyph.ImageBounds.Top) / image.Height,
                    glyph.ImageBounds.Right / image.Width,
                    (image.Height - glyph.ImageBounds.Bottom) / image.Height
                ),
                Advance = new Vector2D<float>(
                    glyph.Advance.Horizontal,
                    glyph.Advance.Vertical
                ) * variant.Metrics.FontSize
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Glyph GetGlyph(uint codepoint)
    {
        return _glyphs[codepoint];
    }

    public readonly record struct Glyph(in Vector2D<float> Dimensions, in Graphics.Rectangle<float> PlaneBounds, in Graphics.Rectangle<float> AtlasCoordinates, in Vector2D<float> Advance)
    {
        public bool IsWhitespace => PlaneBounds is { Left: 0, Right: 0, Top: 0, Bottom: 0 };
    }
}
