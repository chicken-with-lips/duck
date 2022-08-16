using ChickenWithLips.PhysX;
using Duck.Graphics.Components;
using Silk.NET.Maths;

namespace Duck.Physics;

internal static class PhysXHelper
{
    public static PxBoxGeometry CreateBoxGeometry(BoundingBoxComponent box, Vector3D<float> scale)
    {
        return new PxBoxGeometry(box.Box.GetScaled(scale, box.Box.Center).Size.ToSystem() / 2f);
    }
    
    public static PxSphereGeometry CreateSphereGeometry(BoundingSphereComponent sphere, Vector3D<float> scale)
    {
        return new PxSphereGeometry(sphere.Radius * MathF.Max(MathF.Max(scale.X, scale.Y), scale.Z));
    }
}
