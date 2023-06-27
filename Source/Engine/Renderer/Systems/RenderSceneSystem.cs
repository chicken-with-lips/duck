using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Renderer.Components;
using Duck.Renderer.Device;

namespace Duck.Renderer.Systems;

public partial class RenderSceneSystem : BaseSystem<World, float>, IPresentationSystem
{
    public CommandBuffer? RenderCommandBuffer { get; set; }
    public View? View { get; set; }

    private readonly IGraphicsDevice _graphicsDevice;

    public RenderSceneSystem(World world, IGraphicsDevice graphicsDevice)
        : base(world)
    {
        _graphicsDevice = graphicsDevice;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderStaticMeshWithBoundingBox(in TransformComponent transform, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingBoxComponent boundingBox)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("WorldScale", transform.Scale)
            .SetParameter("WorldPosition", transform.WorldTranslation);

        /*var scaledBoundingBox = new BoundingBoxComponent() {
            Box = boundingBox.Box.GetScaled(transform.Scale, boundingBox.Box.Center)
        };*/

        renderObjectInstance.BoundingVolume = boundingBox;

        RenderCommandBuffer?.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderStaticMeshWithBoundingSphere(in TransformComponent transform, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingSphereComponent boundingSphere)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("WorldScale", transform.Scale)
            .SetParameter("WorldPosition", transform.WorldTranslation);

        /*var scaledBoundingSphere = new BoundingSphereComponent() {
            Radius = boundingSphere.Radius
        };*/

        renderObjectInstance.BoundingVolume = boundingSphere;

        RenderCommandBuffer?.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderDirectionalLight(in TransformComponent transform, in DirectionalLightComponent directionalLight)
    {
        RenderCommandBuffer?.AddDirectionalLight(
            transform.Forward,
            directionalLight.Ambient,
            directionalLight.Diffuse,
            directionalLight.Specular
        );
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderPointLight(in TransformComponent transform, in PointLightComponent pointLight)
    {
        Time.PointLightPosition = transform.Position;
        Time.PointLightAmbient = pointLight.Ambient;
        Time.PointLightDiffuse = pointLight.Diffuse;
        Time.PointLightSpecular = pointLight.Specular;
        Time.PointLightConstant = pointLight.Constant;
        Time.PointLightLinear = pointLight.Linear;
        Time.PointLightQuadratic = pointLight.Quadratic;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderSpotLight(in TransformComponent transform, in SpotLightComponent spotLight)
    {
        Time.SpotLightPosition = transform.Position;
        Time.SpotLightDirection = transform.Forward;
        Time.SpotLightAmbient = spotLight.Ambient;
        Time.SpotLightDiffuse = spotLight.Diffuse;
        Time.SpotLightSpecular = spotLight.Specular;
        Time.SpotLightConstant = spotLight.Constant;
        Time.SpotLightLinear = spotLight.Linear;
        Time.SpotLightQuadratic = spotLight.Quadratic;
        Time.SpotLightInnerCutoff = spotLight.InnerCutOff;
        Time.SpotLightOuterCutoff = spotLight.OuterCutOff;
    }
}
