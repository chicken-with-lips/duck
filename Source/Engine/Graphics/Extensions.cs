using System.Drawing;
using Silk.NET.Maths;

namespace Duck.Graphics;

public static class Extensions
{
    public static Vector4D<float> ToVector(this Color color)
    {
        return new Vector4D<float>(
            color.R / 255.0f,
            color.G / 255.0f,
            color.B / 255.0f,
            color.A / 255.0f
        );
    }
}
