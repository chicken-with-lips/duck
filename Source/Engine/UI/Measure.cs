using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance.Helpers;
using Duck.Ui.Elements;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Duck.Ui;

public static class Measure
{
    public const float PixelToOneEm = 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float EmSizeFromPixels(in float px)
    {
        return px / PixelToOneEm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float EmToPixels(float em)
    {
        return em * PixelToOneEm;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BoxArea EmToPixels(in BoxArea boxArea)
    {
        return new BoxArea(
            EmToPixels(boxArea.Top),
            EmToPixels(boxArea.Right),
            EmToPixels(boxArea.Bottom),
            EmToPixels(boxArea.Left)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box EmToPixels(in Box box)
    {
        return new Box(
            EmToPixels(box.ContentWidth),
            EmToPixels(box.ContentHeight),
            EmToPixels(box.Padding),
            EmToPixels(box.Margin)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> EmToPixels(in Vector2D<float> em)
    {
        return new Vector2D<float>(
            EmToPixels(em.X),
            EmToPixels(em.Y)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BoxWidth(in Box box, in bool includeMargin = true)
    {
        var width = box.Padding.Left + box.Padding.Right + box.ContentWidth;

        if (includeMargin) {
            width += box.Margin.Left + box.Margin.Right;
        }

        return width;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BoxHeight(in Box box, in bool includeMargin = true)
    {
        var height = box.Padding.Top + box.Padding.Bottom + box.ContentHeight;

        if (includeMargin) {
            height += box.Margin.Top + box.Margin.Bottom;
        }

        return height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BoxWidthInPixels(in Box box)
    {
        return EmToPixels(
            BoxWidth(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BoxHeightInPixels(in Box box)
    {
        return EmToPixels(
            BoxHeight(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> BoxDimensions(in Box box)
    {
        return new Vector2D<float>(
            BoxWidth(box),
            BoxHeight(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> BoxDimensionsInPixels(in Box box)
    {
        return new Vector2D<float>(
            BoxWidthInPixels(box),
            BoxHeightInPixels(box)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> BoxDimensionsInPixels(in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor }) {
            return BoxDimensionsInPixels(accessor.GetBox(fragment));
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensions(in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IContentAccessor accessor }) {
            return accessor.GetContentDimensions(fragment);
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionsInPixels(in Fragment fragment)
    {
        return EmToPixels(ContentDimensions(fragment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensions(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        var gapSize = new Vector2D<float>(margin.Left, margin.Top);
        var contentDimensions = Vector2D<float>.Zero;
        var childCount = 0;

        if (fragment0 is { PropertyAccessor: IBoxAccessor accessor0 }) {
            contentDimensions += BoxDimensions(accessor0.GetBox(fragment0.Value));
            childCount++;
        }

        if (fragment1 is { PropertyAccessor: IBoxAccessor accessor1 }) {
            contentDimensions += BoxDimensions(accessor1.GetBox(fragment1.Value));
            childCount++;
        }

        if (fragment2 is { PropertyAccessor: IBoxAccessor accessor2 }) {
            contentDimensions += BoxDimensions(accessor2.GetBox(fragment2.Value));
            childCount++;
        }

        if (fragment3 is { PropertyAccessor: IBoxAccessor accessor3 }) {
            contentDimensions += BoxDimensions(accessor3.GetBox(fragment3.Value));
            childCount++;
        }

        if (fragment4 is { PropertyAccessor: IBoxAccessor accessor4 }) {
            contentDimensions += BoxDimensions(accessor4.GetBox(fragment4.Value));
            childCount++;
        }

        if (fragment5 is { PropertyAccessor: IBoxAccessor accessor5 }) {
            contentDimensions += BoxDimensions(accessor5.GetBox(fragment5.Value));
            childCount++;
        }

        return new Vector2D<float>(
            contentDimensions.X + (gapSize.X * MathF.Max(0, childCount - 1)),
            contentDimensions.Y + (gapSize.Y * MathF.Max(0, childCount - 1))
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionsInPixels(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        return EmToPixels(
            ContentDimensions(margin, fragment0, fragment1, fragment2, fragment3, fragment4, fragment5)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPosition(in ElementRenderContext renderContext, in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor }) {
            return ContentPosition(renderContext, accessor.GetBox(fragment));
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPosition(in ElementRenderContext renderContext, in Box box)
    {
        return ContentPosition(renderContext.Position, box);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPosition(in Vector2D<float> basePosition, in Box box)
    {
        return basePosition + new Vector2D<float>(box.Margin.Left + box.Padding.Left, box.Margin.Top + box.Padding.Top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPositionInPixels(in Vector2D<float> basePosition, in Box box)
    {
        return EmToPixels(ContentPosition(basePosition, box));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPositionInPixels(in ElementRenderContext renderContext, in Box box)
    {
        return EmToPixels(ContentPosition(renderContext, box));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPositionInPixels(in ElementRenderContext renderContext, in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor }) {
            return ContentPositionInPixels(renderContext.Position, accessor.GetBox(fragment));
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPosition(in ElementRenderContext renderContext, in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor }) {
            return ElementPosition(renderContext, accessor.GetBox(fragment));
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPosition(in ElementRenderContext renderContext, in Box box)
    {
        return ElementPosition(renderContext.Position, box);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPosition(in Vector2D<float> basePosition, in Box box)
    {
        return basePosition + new Vector2D<float>(box.Margin.Left, box.Margin.Top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPositionInPixels(in Vector2D<float> basePosition, in Box box)
    {
        return EmToPixels(ElementPosition(basePosition, box));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPositionInPixels(in ElementRenderContext renderContext, in Box box)
    {
        return EmToPixels(ElementPosition(renderContext, box));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPositionInPixels(in ElementRenderContext renderContext, in Fragment fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor }) {
            return ElementPositionInPixels(renderContext.Position, accessor.GetBox(fragment));
        }

        return Vector2D<float>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointInside(in ElementRenderContext renderContext, in Fragment fragment, in Vector2D<float> point)
    {
        var position = ElementPositionInPixels(renderContext, fragment);
        var dimensions = BoxDimensionsInPixels(fragment);

        return Math.MathF.IsPointInside<float>(point, new Box2D<float>(position, position + dimensions));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointInside(in ElementRenderContext renderContext, in Fragment fragment, in Vector2D<int> point)
    {
        return IsPointInside(renderContext, fragment, point.As<float>());
    }
}
