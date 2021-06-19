using System.Numerics;

namespace Duck.Contracts.Math
{
    public static class VectorEx
    {
        public const float Epsilon = 0.00001f;
        public const float EpsilonNormalSqrt = 1e-15f;

        // FIXME: it is not safe to use this code
        public static float Angle(Vector3 from, Vector3 to)
        {
            float denominator = (float)Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            float dot = Mathf.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return ((float)Math.Acos(dot)) * Mathf.Rad2Deg;
        }
    }
}
