using System.Runtime.CompilerServices;
using ADyn.Components;
using Silk.NET.Maths;

namespace Duck.Math;

public static class Transform
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Forward(in Position position, in Orientation orientation)
    {
        return Forward(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Forward(in PresentPosition position, in PresentOrientation orientation)
    {
        return Forward(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Forward(in Vector3D<float> position, in Quaternion<float> orientation)
    {
        return Vector3D.Normalize(Vector3D.Transform(Vector3D<float>.UnitZ, orientation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Right(in Position position, in Orientation orientation)
    {
        return Right(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Right(in PresentPosition position, in PresentOrientation orientation)
    {
        return Right(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Right(in Vector3D<float> position, in Quaternion<float> orientation)
    {
        return Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, Forward(position, orientation)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Up(in Position position, in Orientation orientation)
    {
        return Up(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Up(in PresentPosition position, in PresentOrientation orientation)
    {
        return Up(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<float> Up(in Vector3D<float> position, in Quaternion<float> orientation)
    {
        return Vector3D.Cross(Forward(position, orientation), Right(position, orientation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Matrix4X4<float> CreateLookAtMatrix(in Position position, in Orientation orientation)
    {
        return CreateLookAtMatrix(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Matrix4X4<float> CreateLookAtMatrix(in PresentPosition position, in PresentOrientation orientation)
    {
        return CreateLookAtMatrix(position.Value, orientation.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Matrix4X4<float> CreateLookAtMatrix(in Vector3D<float> position, in Quaternion<float> orientation)
    {
        return Matrix4X4.CreateLookAt(
            position,
            position + Forward(position, orientation),
            Up(position, orientation)
        );
    }
}
