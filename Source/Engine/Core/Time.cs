using Duck.Platform;
using Silk.NET.Maths;

namespace Duck;

public static class Time
{
    public static float Elapsed => FrameTimer?.Elapsed ?? 0;
    public static float DeltaFrame => FrameTimer?.Delta ?? 0;
    public static double DoubleDeltaFrame => FrameTimer?.DoubleDelta ?? 0;

    public static IFrameTimer? FrameTimer { get; set; }


    public static Vector3D<float> DirectionalLightDirection { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> DirectionalLightAmbient { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> DirectionalLightDiffuse { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> DirectionalLightSpecular { get; set; } = Vector3D<float>.Zero;

    public static Vector3D<float> PointLightPosition { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> PointLightAmbient { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> PointLightDiffuse { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> PointLightSpecular { get; set; } = Vector3D<float>.Zero;
    public static float PointLightConstant = 0f;
    public static float PointLightLinear = 0f;
    public static float PointLightQuadratic = 0.032f;

    public static Vector3D<float> SpotLightPosition { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> SpotLightDirection { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> SpotLightAmbient { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> SpotLightDiffuse { get; set; } = Vector3D<float>.Zero;
    public static Vector3D<float> SpotLightSpecular { get; set; } = Vector3D<float>.Zero;
    public static float SpotLightConstant = 0f;
    public static float SpotLightLinear = 0f;
    public static float SpotLightQuadratic = 0;
    public static float SpotLightInnerCutoff = 0;
    public static float SpotLightOuterCutoff = 0;

    public static Vector3D<float> CameraPosition { get; set; } = Vector3D<float>.Zero;
}
