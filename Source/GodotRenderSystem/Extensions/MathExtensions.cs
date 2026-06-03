using Godot;
using Silk.NET.Maths;

namespace Duck.RenderSystem.Godot.Extensions;

public static class MathExtensions
{
    public static Vector2D<int> ToVector2D(this Vector2I value) => new(value.X, value.Y);

    public static Vector3 ToGodot(this Vector3D<float> value) => new(value.X, value.Y, value.Z);
}