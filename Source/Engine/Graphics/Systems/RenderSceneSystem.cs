using System.Runtime.CompilerServices;
using ADyn.Components;
using Arch.Core;
using Arch.System;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Math;
using Duck.Platform;
using Silk.NET.Maths;
using Transform = Duck.Math.Transform;

namespace Duck.Graphics.Systems;

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
    public void RenderStaticMeshWithBoundingBox(in LocalTransform localTransform, in LocalToWorld localToWorld, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingBoxComponent boundingBox)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("Transform", localToWorld.Value);

        /*var scaledBoundingBox = new BoundingBoxComponent() {
            Box = boundingBox.Box.GetScaled(transform.Scale, boundingBox.Box.Center)
        };*/

        renderObjectInstance.BoundingVolume = boundingBox;

        RenderCommandBuffer?.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderStaticMeshWithBoundingSphere(in LocalToWorld localToWorld, in RuntimeStaticMeshComponent runtimeStaticMesh, in BoundingSphereComponent boundingSphere)
    {
        var renderObjectInstance = _graphicsDevice.GetRenderObjectInstance(runtimeStaticMesh.InstanceId);
        renderObjectInstance
            .SetParameter("Transform", localToWorld.Value);

        /*var scaledBoundingSphere = new BoundingSphereComponent() {
            Radius = boundingSphere.Radius
        };*/

        renderObjectInstance.BoundingVolume = boundingSphere;

        RenderCommandBuffer?.ScheduleRenderableInstance(runtimeStaticMesh.InstanceId);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderDirectionalLight(in LocalTransform localTransform, in DirectionalLightComponent directionalLight)
    {
        RenderCommandBuffer?.AddDirectionalLight(
            localTransform.Forward,
            directionalLight.Ambient,
            directionalLight.Diffuse,
            directionalLight.Specular
        );
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderPointLight(in LocalTransform localTransform, in PointLightComponent pointLight)
    {
        Time.PointLightPosition = localTransform.Position;
        Time.PointLightAmbient = pointLight.Ambient;
        Time.PointLightDiffuse = pointLight.Diffuse;
        Time.PointLightSpecular = pointLight.Specular;
        Time.PointLightConstant = pointLight.Constant;
        Time.PointLightLinear = pointLight.Linear;
        Time.PointLightQuadratic = pointLight.Quadratic;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderSpotLight(in LocalTransform localTransform, in SpotLightComponent spotLight)
    {
        Time.SpotLightPosition = localTransform.Position;
        Time.SpotLightDirection = localTransform.Forward;
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
