using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Duck.Ui.Elements;

public readonly unsafe struct Fragment
{
    public readonly void* Element;
    public readonly IElementRenderer ElementRenderer;
    public readonly IElementPropertyAccessor? PropertyAccessor;

    public Fragment(void* element, IElementRenderer elementRenderer, IElementPropertyAccessor? propertyAccessor)
    {
        Element = element;
        ElementRenderer = elementRenderer;
        PropertyAccessor = propertyAccessor;
    }

    [Pure]
    public ref T GetElementAs<T>()
    {
        return ref Unsafe.AsRef<T>(Element);
    }

    public static Fragment From<T>(ref T element, IElementRenderer elementRenderer, IElementPropertyAccessor? propertyAccessor = null) where T : struct
    {
        return new(Unsafe.AsPointer(ref element), elementRenderer, propertyAccessor);
    }
}

public readonly record struct Box(in float ContentWidth, in float ContentHeight, in BoxArea Padding, in BoxArea Margin)
{
    public static readonly Box Default = new(0, 0, BoxArea.Default, BoxArea.Default);

    public static Box operator +(Box a) => a;

    public static Box operator -(Box a)
    {
        return new Box(
            -a.ContentWidth,
            -a.ContentHeight,
            -a.Padding,
            -a.Margin
        );
    }

    public static Box operator +(Box a, Box b)
    {
        return new Box(
            MathF.Max(0, a.ContentWidth + b.ContentWidth),
            MathF.Max(0, a.ContentHeight + b.ContentHeight),
            a.Padding + b.Padding,
            a.Margin + b.Margin
        );
    }

    public static Box operator -(Box a, Box b) => a + (-b);

    public static Box operator *(Box a, Box b)
    {
        return new Box(
            MathF.Max(0, a.ContentWidth * b.ContentWidth),
            MathF.Max(0, a.ContentHeight * b.ContentHeight),
            a.Padding * b.Padding,
            a.Margin * b.Margin
        );
    }

    public static Box operator /(Box a, Box b)
    {
        return new Box(
            MathF.Max(0, a.ContentWidth / b.ContentWidth),
            MathF.Max(0, a.ContentHeight / b.ContentHeight),
            a.Padding / b.Padding,
            a.Margin / b.Margin
        );
    }
}

public readonly record struct BoxArea(in float Top, in float Right, in float Bottom, in float Left)
{
    public static readonly BoxArea Default = new();

    public static BoxArea VerticalHorizontal(in float vertical, in float horizontal)
    {
        return new(vertical, horizontal, vertical, horizontal);
    }

    public static BoxArea All(in float value)
    {
        return new(value, value, value, value);
    }

    public static BoxArea operator +(BoxArea a) => a;

    public static BoxArea operator -(BoxArea a)
    {
        return new BoxArea(
            -a.Top,
            -a.Right,
            -a.Bottom,
            -a.Left
        );
    }

    public static BoxArea operator +(BoxArea a, BoxArea b)
    {
        return new BoxArea(
            MathF.Max(0, a.Top + b.Top),
            MathF.Max(0, a.Right + b.Right),
            MathF.Max(0, a.Bottom + b.Bottom),
            MathF.Max(0, a.Left + b.Left)
        );
    }

    public static BoxArea operator -(BoxArea a, BoxArea b) => a + (-b);

    public static BoxArea operator *(BoxArea a, BoxArea b)
    {
        return new BoxArea(
            MathF.Max(0, a.Top * b.Top),
            MathF.Max(0, a.Right * b.Right),
            MathF.Max(0, a.Bottom * b.Bottom),
            MathF.Max(0, a.Left * b.Left)
        );
    }

    public static BoxArea operator /(BoxArea a, BoxArea b)
    {
        return new BoxArea(
            MathF.Max(0, a.Top / b.Top),
            MathF.Max(0, a.Right / b.Right),
            MathF.Max(0, a.Bottom / b.Bottom),
            MathF.Max(0, a.Left / b.Left)
        );
    }
}

public enum HorizontalAlign
{
    Center,
    Left,
    Right,
}

public enum VerticalAlign
{
    Bottom,
    Center,
    Top,
}
