using System.Numerics;
using System.Runtime.CompilerServices;
using Duck.Math;

namespace Duck.Renderer;

public readonly record struct Rectangle<T>(in T Left, in T Top, in T Right, in T Bottom)
    where T : struct, INumber<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rectangle<T> Scale(T value)
    {
        return new Rectangle<T>(
            Left * value,
            Top * value,
            Right * value,
            Bottom * value
        );
    }
}
