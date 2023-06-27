using Duck.Renderer.Components;
using Duck.Math;
using Duck.Serialization;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Duck.Renderer.Components;

[AutoSerializable]
public partial struct CameraComponent
{
    public bool IsActive = true;

    public float FieldOfView = 75f;
    public float AspectRatio = 16f / 9f;
    public float NearClipPlane = 0.1f;
    public float FarClipPlane = 1000f;

    public CameraComponent()
    {
    }

    public Vector3D<float> ScreenToWorldPosition(TransformComponent transformComponent, Vector3D<float> screenCoordinates)
    {
        // https://antongerdelan.net/opengl/raycasting.html
        
        // FIXME: fixed to 1280x1024

        var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathF.ToRadians(FieldOfView), 1280f / 1024f, NearClipPlane, FarClipPlane);
        var viewMatrix = Matrix4X4.CreateLookAt(transformComponent.Position, transformComponent.Position + transformComponent.Forward, transformComponent.Up);

        var x = (2f * screenCoordinates.X) / 1280f - 1f;
        var y = 1f - (2f * screenCoordinates.Y) / 1024f;
        var z = screenCoordinates.Z;
        var rayNormalizedDeviceCoordinates = new Vector3D<float>(x, y, z);
        
        // 4D homogeneous clip coordinates
        var rayClip = new Vector4D<float>(rayNormalizedDeviceCoordinates.X, rayNormalizedDeviceCoordinates.Y, -1f, 1f);
        
        // 4D eye (camera) coordinates
        Matrix4X4.Invert(projectionMatrix, out var invertedProjectionMatrix);
        var rayEye = Vector4D.Transform(rayClip, invertedProjectionMatrix);
        rayEye = new Vector4D<float>(rayEye.X, rayEye.Y, -1f, 0f);
        
        // 4D world coordinates
        Matrix4X4.Invert(viewMatrix, out var invertedViewMatrix);
        var rayWorldCoordinates = Vector4D.Transform(rayEye, invertedViewMatrix);

        return Vector3D.Normalize(new Vector3D<float>(rayWorldCoordinates.X, rayWorldCoordinates.Y, rayWorldCoordinates.Z));
    }
}
