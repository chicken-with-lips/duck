using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Duck.Math;

// Mostly sourced from https://github.com/kermado/M3D
public static class MathHelper
{
    public const float Deg2Rad = (float)System.Math.PI / 180f;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(Vector3D<float> from, Vector3D<float> to)
    {
        float cosTheta = Vector3D.Dot(from, to) / MathF.Sqrt(from.LengthSquared * to.LengthSquared);
        return MathF.Acos(MathF.Min(1.0f, cosTheta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(Quaternion<float> from, Quaternion<float> to)
    {
        Quaternion<float> relativeRotation = Quaternion<float>.Conjugate(from) * to;
        Debug.Assert(MathF.Abs(relativeRotation.W) <= 1.0f);

        return 2.0f * MathF.Acos(relativeRotation.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<float> RotateTowards(Quaternion<float> from, Quaternion<float> to, float maxRadiansDelta)
    {
        // Relative unit quaternion rotation between this quaternion and the
        // target quaternion.
        Quaternion<float> relativeRotation = Quaternion<float>.Conjugate(from) * to;

        // Calculate the angle and axis of the relative quaternion rotation.
        Debug.Assert(MathF.Abs(relativeRotation.W) <= 1.0f);

        float angle = 2.0f * MathF.Acos(relativeRotation.W);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToRadians(float degrees)
    {
        return degrees * Deg2Rad;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToDegrees(float radians)
    {
        return radians * 57.295776f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            return Quaternion<float>.CreateFromAxisAngle(Vector3D.Normalize(axis), MathF.PI);
        } else {
            // Scalar component.
            float s = MathF.Sqrt(unitFrom.LengthSquared * unitTo.LengthSquared) + Vector3D.Dot(unitFrom, unitTo);

            // Vector component.
            Vector3D<float> v = Vector3D.Cross(unitFrom, unitTo);

            // Return the normalized quaternion rotation.
            return Quaternion<float>.Normalize(new Quaternion<float>(v, s));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<float> LookRotation(Vector3D<float> forward)
    {
        Debug.Assert(forward.LengthSquared > 0.0f);
        return FromToRotation(-Vector3D<float>.UnitZ, forward);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public static void ToAngleAxis(Quaternion<float> q, out float angle, out Vector3D<float> axis)
    {
        angle = 2 * MathF.Acos(q.W);
        axis = new Vector3D<float>(
            q.X / MathF.Sqrt(1 - q.W * q.W),
            q.Y / MathF.Sqrt(1 - q.W * q.W),
            q.Z / MathF.Sqrt(1 - q.W * q.W)
        );
    }
}
