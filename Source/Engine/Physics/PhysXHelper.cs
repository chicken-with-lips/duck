using ChickenWithLips.PhysX;
using Duck.Renderer.Components;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Duck.Physics;

internal static class PhysXHelper
{
    public static PxBoxGeometry CreateBoxGeometry(in Box3D<float> box, in Vector3D<float> scale)
    {
        return new PxBoxGeometry(box.GetScaled(scale, box.Center).Size.ToSystem() / 2f);
    }
    
    public static PxSphereGeometry CreateSphereGeometry(in BoundingSphereComponent sphere, in Vector3D<float> scale)
    {
        return new PxSphereGeometry(sphere.Radius * MathF.Max(MathF.Max(scale.X, scale.Y), scale.Z));
    }
}
