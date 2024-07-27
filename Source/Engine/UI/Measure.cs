using System.Runtime.CompilerServices;
using Duck.Ui.Elements;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

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
    public static Vector2D<float> BoxDimensions(in Fragment? fragment)
    {
        if (!fragment.HasValue) {
            return default;
        }

        var box = Box(fragment);

        return new Vector2D<float>(
            BoxWidth(box),
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
        return BoxDimensionsInPixels(Box(fragment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensions(in Fragment fragment)
    {
        var box = Box(fragment);

        return new Vector2D<float>(
            box.ContentWidth,
            box.ContentHeight
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionsInPixels(in Fragment fragment)
    {
        return EmToPixels(ContentDimensions(fragment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionsHorizontal(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null, bool isBlockLevel = true)
    {
        return ContentDimensions(
            margin,
            (in Fragment fragment, ref Vector2D<float> vector) => {
                var dim = BoxDimensions(fragment);
                vector.X += dim.X;
                vector.Y = MathF.Max(vector.Y, dim.Y);
            },
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionsVertical(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null, bool isBlockLevel = true)
    {
        return ContentDimensions(
            margin,
            (in Fragment fragment, ref Vector2D<float> vector) => {
                var dim = BoxDimensions(fragment);
                vector.X = MathF.Max(vector.X, dim.X);
                vector.Y += dim.Y;
            },
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensions(in BoxArea margin, ModifyVectorCallback modifyVectorCallback, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        var gapSize = new Vector2D<float>(margin.Left, margin.Top);
        var contentDimensions = Vector2D<float>.Zero;
        var childCount = 0;

        if (fragment0.HasValue) {
            modifyVectorCallback(fragment0.Value, ref contentDimensions);
            childCount++;
        }

        if (fragment1.HasValue) {
            modifyVectorCallback(fragment1.Value, ref contentDimensions);
            childCount++;
        }

        if (fragment2.HasValue) {
            modifyVectorCallback(fragment2.Value, ref contentDimensions);
            childCount++;
        }

        if (fragment3.HasValue) {
            modifyVectorCallback(fragment3.Value, ref contentDimensions);
            childCount++;
        }

        if (fragment4.HasValue) {
            modifyVectorCallback(fragment4.Value, ref contentDimensions);
            childCount++;
        }

        if (fragment5.HasValue) {
            modifyVectorCallback(fragment5.Value, ref contentDimensions);
            childCount++;
        }

        return new Vector2D<float>(
            contentDimensions.X + (gapSize.X * System.MathF.Max(0, childCount - 1)),
            contentDimensions.Y + (gapSize.Y * System.MathF.Max(0, childCount - 1))
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionHorizontalInPixels(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        return EmToPixels(
            ContentDimensionsHorizontal(margin, fragment0, fragment1, fragment2, fragment3, fragment4, fragment5)
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentDimensionVerticalInPixels(in BoxArea margin, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        return EmToPixels(
            ContentDimensionsVertical(margin, fragment0, fragment1, fragment2, fragment3, fragment4, fragment5)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ContentPosition(in ElementRenderContext renderContext, in Fragment fragment)
    {
        return ContentPosition(renderContext, Box(fragment));
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
        return ContentPositionInPixels(renderContext.Position, Box(fragment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D<float> ElementPosition(in ElementRenderContext renderContext, in Fragment fragment)
    {
        return ElementPosition(renderContext, Box(fragment));
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
        return ElementPositionInPixels(renderContext.Position, Box(fragment));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointInside(in ElementRenderContext renderContext, in Fragment fragment, in Vector2D<float> point)
    {
        var position = ElementPositionInPixels(renderContext, fragment);
        var dimensions = BoxDimensionsInPixels(fragment);

        return MathF.IsPointInside<float>(point, new Box2D<float>(position, position + dimensions));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointInside(in ElementRenderContext renderContext, in Fragment fragment, in Vector2D<int> point)
    {
        return IsPointInside(renderContext, fragment, point.As<float>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Box Box(in Fragment? fragment)
    {
        if (fragment is { PropertyAccessor: IBoxAccessor accessor0 }) {
            return accessor0.GetBox(fragment.Value);
        }

        if (fragment is { PropertyAccessor: IContentAccessor accessor1 }) {
            var contentDimensions = accessor1.GetContentDimensions(fragment.Value);

            return Elements.Box.Default with {
                ContentWidth = contentDimensions.X,
                ContentHeight = contentDimensions.Y,
            };
        }

        return Elements.Box.Default;
    }


    public delegate void ModifyVectorCallback(in Fragment fragment, ref Vector2D<float> vector);
}
