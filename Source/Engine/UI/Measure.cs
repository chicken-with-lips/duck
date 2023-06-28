using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Helpers;
using Duck.Ui.Elements;
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
    public static BoxArea ConvertEmToPixels(in BoxArea boxArea)
    {
        return new BoxArea(
            ConvertEmToPixels(boxArea.Top),
            ConvertEmToPixels(boxArea.Right),
            ConvertEmToPixels(boxArea.Bottom),
            ConvertEmToPixels(boxArea.Left)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box ConvertEmToPixels(in Box box)
    {
        return new Box(
            ConvertEmToPixels(box.ContentWidth),
            ConvertEmToPixels(box.ContentHeight),
            ConvertEmToPixels(box.Padding),
            ConvertEmToPixels(box.Margin)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ConvertEmToPixels(in Vector2D<float> em)
    {
        return new Vector2D<float>(
            ConvertEmToPixels(em.X),
            ConvertEmToPixels(em.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxWidth(in Box box)
    {
        return box.ContentWidth + box.Margin.Left + box.Margin.Right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxHeight(in Box box)
    {
        return box.ContentHeight + box.Margin.Top + box.Margin.Bottom;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxWidthWithPadding(in Box box)
    {
        return CalculateBoxWidth(box) + box.Padding.Left + box.Padding.Right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxHeightWithPadding(in Box box)
    {
        return CalculateBoxHeight(box) + box.Padding.Top + box.Padding.Bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxWidthInPixels(in Box box)
    {
        return ConvertEmToPixels(
            CalculateBoxWidth(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxHeightInPixels(in Box box)
    {
        return ConvertEmToPixels(
            CalculateBoxHeight(box)
        );
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxWidthInPixelsWithPadding(in Box box)
    {
        return CalculateBoxWidthInPixels(box) + ConvertEmToPixels(box.Padding.Left) + ConvertEmToPixels(box.Padding.Right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateBoxHeightInPixelsWithPadding(in Box box)
    {
        return CalculateBoxHeightInPixels(box) + ConvertEmToPixels(box.Padding.Top) + ConvertEmToPixels(box.Padding.Bottom);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> CalculateBoxDimensions(in Box box)
    {
        return new Vector2D<float>(
            CalculateBoxWidth(box),
            CalculateBoxHeight(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> CalculateBoxDimensionsInPixels(in Box box)
    {
        return new Vector2D<float>(
            CalculateBoxWidthInPixels(box),
            CalculateBoxHeightInPixels(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> CalculateBoxDimensionsInPixelsWithPadding(in Box box)
    {
        return new Vector2D<float>(
            CalculateBoxWidthInPixelsWithPadding(box),
            CalculateBoxHeightInPixelsWithPadding(box)
        );
    }
}
