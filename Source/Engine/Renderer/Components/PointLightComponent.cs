using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Renderer.Components;

[AutoSerializable]
public partial struct PointLightComponent
{
    public Vector3D<float> Ambient = default;
    public Vector3D<float> Diffuse = default;
    public Vector3D<float> Specular = default;

    public float Constant = default;
    public float Linear = default;
    public float Quadratic = default;

    public PointLightComponent()
    {
    }
}
