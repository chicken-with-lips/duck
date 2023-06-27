using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Duck.Ui;

public static class Measure
{
    public const float PixelToOneEm = 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateEmSizeFromPixels(in float px)
    {
        return px / PixelToOneEm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertEmToPixels(float em)
    {
        return em * PixelToOneEm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ConvertEmToPixels(in Vector2D<float> em)
    {
        return new Vector2D<float>(
            ConvertEmToPixels(em.X),
            ConvertEmToPixels(em.Y)
        );
    }
}
