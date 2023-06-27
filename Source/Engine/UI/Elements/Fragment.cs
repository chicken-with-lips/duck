using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

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
    public static readonly Box Default = new(1, 1, BoxArea.Default, BoxArea.Default);

    public Vector2D<float> ToVector() => new(ContentWidth, ContentHeight);
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
}
