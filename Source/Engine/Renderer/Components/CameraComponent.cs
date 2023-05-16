using Duck.Renderer.Components;
using Duck.Math;
using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Renderer.Components;

[AutoSerializable]
public partial struct CameraComponent
{
    public bool IsActive = true;

    public float FieldOfView = 60f;
    public float AspectRatio = 16f / 9f;
    public float NearClipPlane = 0.1f;
    public float FarClipPlane = 1000f;

    public CameraComponent()
    {
    }

    public Vector3D<float> ScreenToWorldPosition(TransformComponent transformComponent, Vector3D<float> screenCoordinates)
    {
        if (screenCoordinates.LengthSquared > 0) {
        }

        // FIXME: fixed to 1280x1024
        throw new Exception("ADD TO VIEW");
        // // Define the camera's projection matrix
        var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView), 1280f / 1024f, NearClipPlane, FarClipPlane);
        //
        // // Define the camera's view matrix
        var viewMatrix = Matrix4X4.CreateLookAt(transformComponent.Position, transformComponent.Position + transformComponent.Forward, transformComponent.Up);
        //
        // // Convert the mouse position from screen space to homogeneous clip space
        // // var mousePosition = new Vector3D<float>(screenCoordinates.X, screenCoordinates.Y, 0);
        // var mousePosition = new Vector3D<float>(screenCoordinates.X, 0, screenCoordinates.Y);
        // var mousePositionClipSpace = Vector4D.Transform(mousePosition, viewMatrix * projectionMatrix);
        //
        // // Divide the x, y, and z coordinates of the clip space position by the w coordinate
        // var mousePositionNormalizedDeviceCoordinates = new Vector3D<float>(
        //     mousePositionClipSpace.X / mousePositionClipSpace.W,
        //     mousePositionClipSpace.Y / mousePositionClipSpace.W,
        //     mousePositionClipSpace.Z / mousePositionClipSpace.W
        // );
        //
        // Matrix4X4.Invert(viewMatrix, out var inverseViewMatrix);
        //
        // // Convert the normalized device coordinates to world space
        // return Vector3D.Transform(mousePositionNormalizedDeviceCoordinates, inverseViewMatrix);

        // heavily influenced by: http://antongerdelan.net/opengl/raycasting.html
        // viewport coordinate system
        // normalized device coordinates

        // FIXME: fixed to 1280x1024
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
