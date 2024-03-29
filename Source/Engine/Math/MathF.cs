using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Duck.Math;

// Mostly sourced from https://github.com/kermado/M3D
public static class MathF
{
    public const float PI = System.MathF.PI;
    public const float Deg2Rad = System.MathF.PI / 180f;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Acos(float x) => System.MathF.Acos(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Sqrt(float x) => System.MathF.Sqrt(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Min(float x, float y) => System.MathF.Min(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Max(float x, float y) => System.MathF.Max(x, y);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Abs(float x) => System.MathF.Abs(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Cos(float x) => System.MathF.Cos(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Sin(float x) => System.MathF.Sin(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T Lerp<T>(T start, T end, T percent) where T : unmanaged
    {
        return Scalar.Add(start, Scalar.Multiply(Scalar.Subtract(end, start), percent));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2D<T> Lerp<T>(Vector2D<T> start, Vector2D<T> end, T percent) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Vector2D<T>(
            Lerp(start.X, end.X, percent),
            Lerp(start.Y, end.Y, percent)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<T> Lerp<T>(Vector3D<T> start, Vector3D<T> end, T percent) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Vector3D<T>(
            Lerp(start.X, end.X, percent),
            Lerp(start.Y, end.Y, percent),
            Lerp(start.Z, end.Z, percent)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Angle(Vector3D<float> from, Vector3D<float> to)
    {
        float cosTheta = Vector3D.Dot(from, to) / Sqrt(from.LengthSquared * to.LengthSquared);
        return Acos(Min(1.0f, cosTheta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Angle(Quaternion<float> from, Quaternion<float> to)
    {
        Quaternion<float> relativeRotation = Quaternion<float>.Conjugate(from) * to;
        Debug.Assert(Abs(relativeRotation.W) <= 1.0f);

        return 2.0f * Acos(relativeRotation.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Quaternion<float> RotateTowards(Quaternion<float> from, Quaternion<float> to, float maxRadiansDelta)
    {
        // Relative unit quaternion rotation between this quaternion and the
        // target quaternion.
        Quaternion<float> relativeRotation = Quaternion<float>.Conjugate(from) * to;

        // Calculate the angle and axis of the relative quaternion rotation.
        Debug.Assert(Abs(relativeRotation.W) <= 1.0f);

        float angle = 2.0f * Acos(relativeRotation.W);
        Vector3D<float> axis = new Vector3D<float>(relativeRotation.X, relativeRotation.Y, relativeRotation.Z);

        // Apply a step of the relative rotation.
        if (angle > maxRadiansDelta) {
            // If the angle between the two quaternions is greater than the
            // maximum amount we are allowed to rotate by, then rotate by
            // maxRadiansDelta. Note that we need to normalize the axis as the
            // vector part of the relativeRotation quaternion is probably not
            // a unit vector (unless the scalar part is zero).
            Quaternion<float> delta = Quaternion<float>.CreateFromAxisAngle(Vector3D.Normalize(axis), maxRadiansDelta);

            return delta * from;
        }

        // We would overshoot, so just set this quaternion to the target.
        return to;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ToRadians(float degrees)
    {
        return degrees * Deg2Rad;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float ToDegrees(float radians)
    {
        return radians * 57.295776f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Quaternion<float> FromToRotation(Vector3D<float> fromDirection, Vector3D<float> toDirection)
    {
        Debug.Assert(fromDirection.LengthSquared > 0.0f && toDirection.LengthSquared > 0.0f);

        Vector3D<float> unitFrom = Vector3D.Normalize(fromDirection);
        Vector3D<float> unitTo = Vector3D.Normalize(toDirection);
        float d = Vector3D.Dot(unitFrom, unitTo);

        if (d >= 1.0f) {
            // In the case where the two vectors are pointing in the same
            // direction, we simply return the identity rotation.
            return Quaternion<float>.Identity;
        } else if (d <= -1.0f) {
            // If the two vectors are pointing in opposite directions then we
            // need to supply a quaternion corresponding to a rotation of
            // PI-radians about an axis orthogonal to the fromDirection.
            Vector3D<float> axis = Vector3D.Cross(unitFrom, Vector3D<float>.UnitX);

            if (axis.LengthSquared < 1e-6f) {
                // Bad luck. The x-axis and fromDirection are linearly
                // dependent (colinear). We'll take the axis as the vector
                // orthogonal to both the y-axis and fromDirection instead.
                // The y-axis and fromDirection will clearly not be linearly
                // dependent.
                axis = Vector3D.Cross(unitFrom, Vector3D<float>.UnitY);
            }

            // Note that we need to normalize the axis as the cross product of
            // two unit vectors is not necessarily a unit vector.
            return Quaternion<float>.CreateFromAxisAngle(Vector3D.Normalize(axis), PI);
        } else {
            // Scalar component.
            float s = Sqrt(unitFrom.LengthSquared * unitTo.LengthSquared) + Vector3D.Dot(unitFrom, unitTo);

            // Vector component.
            Vector3D<float> v = Vector3D.Cross(unitFrom, unitTo);

            // Return the normalized quaternion rotation.
            return Quaternion<float>.Normalize(new Quaternion<float>(v, s));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Quaternion<float> LookRotation(Vector3D<float> forward)
    {
        Debug.Assert(forward.LengthSquared > 0.0f);
        return FromToRotation(-Vector3D<float>.UnitZ, forward);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Quaternion<float> LookRotation(Vector3D<float> forward, Vector3D<float> upwards)
    {
        // Calculate the unit quaternion that rotates Vector3D<float>::FORWARD to face
        // in the specified forward direction.
        Quaternion<float> q1 = LookRotation(forward);

        // We can't preserve the upwards direction if the forward and upwards
        // vectors are linearly dependent (colinear).
        if (Vector3D.Cross(forward, upwards).LengthSquared < 1e-6f) {
            return q1;
        }

        // Determine the upwards direction obtained after applying q1.
        Vector3D<float> newUp = Vector3D.Transform(Vector3D<float>.UnitY, q1);

        // Calculate the unit quaternion rotation that rotates the newUp
        // direction to look in the specified upward direction.
        Quaternion<float> q2 = FromToRotation(newUp, upwards);

        // Return the combined rotation so that we first rotate to look in the
        // forward direction and then rotate to align Vector3D<float>::UPWARD with the
        // specified upward direction. There is no need to normalize the result
        // as both q1 and q2 are unit quaternions.
        return q2 * q1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ToAngleAxis(Quaternion<float> q, out float angle, out Vector3D<float> axis)
    {
        var divider = Sqrt(1.0f - q.W * q.W);

        angle = 2 * Acos(q.W);
        angle = ToDegrees(angle);

        if (divider != 0.0f) {
            axis = new Vector3D<float>(
                q.X / divider,
                q.Y / divider,
                q.Z / divider
            );
        } else {
            axis = new Vector3D<float>(1, 0, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<T> Multiply<T>(Quaternion<T> q, Vector3D<T> v) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        // https://github.com/schteppe/cannon.js/blob/master/src/math/Quaternion.js#L249

        var x = v.X;
        var y = v.Y;
        var z = v.Z;

        var qx = q.X;
        var qy = q.Y;
        var qz = q.Z;
        var qw = q.W;

        // q*v
        var ix = Scalar.Subtract(Scalar.Add(Scalar.Multiply(qw, x), Scalar.Multiply(qy, z)), Scalar.Multiply(qz, y));
        var iy = Scalar.Subtract(Scalar.Add(Scalar.Multiply(qw, y), Scalar.Multiply(qy, x)), Scalar.Multiply(qz, z));
        var iz = Scalar.Subtract(Scalar.Add(Scalar.Multiply(qw, z), Scalar.Multiply(qy, y)), Scalar.Multiply(qz, x));
        var iw = Scalar.Subtract(Scalar.Subtract(Scalar.Multiply(Scalar.Negate(qx), x), Scalar.Multiply(qy, y)), Scalar.Multiply(qz, z));

        return new Vector3D<T>(
            Scalar.Subtract(Scalar.Add(Scalar.Add(Scalar.Multiply(ix, qw), Scalar.Multiply(iw, Scalar.Negate(qx))), Scalar.Multiply(iy, Scalar.Negate(qz))), Scalar.Multiply(iz, Scalar.Negate(qy))),
            Scalar.Subtract(Scalar.Add(Scalar.Add(Scalar.Multiply(iy, qw), Scalar.Multiply(iw, Scalar.Negate(qy))), Scalar.Multiply(iz, Scalar.Negate(qx))), Scalar.Multiply(ix, Scalar.Negate(qz))),
            Scalar.Subtract(Scalar.Add(Scalar.Add(Scalar.Multiply(iz, qw), Scalar.Multiply(iw, Scalar.Negate(qz))), Scalar.Multiply(ix, Scalar.Negate(qy))), Scalar.Multiply(iy, Scalar.Negate(qx)))
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsPointInside<T>(in Vector2D<float> point, in Box2D<float> box) where T : unmanaged
    {
        return box.Contains(point);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3D<T> Clamp<T>(in Vector3D<T> value, in T min, in T max) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var lengthSquared = value.LengthSquared;

        if (Scalar.LessThan(lengthSquared, Scalar.Multiply(min, max))) {
            return value * Scalar.Multiply(Scalar.Sqrt(lengthSquared), min);
        }

        if (Scalar.GreaterThan(lengthSquared, Scalar.Multiply(max, max))) {
            return value * Scalar.Multiply(Scalar.Sqrt(lengthSquared), max);
        }

        return value;
    }
}
