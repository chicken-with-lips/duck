using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct DirectionalLightComponent
{
    public Vector3D<float> Ambient = default;
    public Vector3D<float> Diffuse = default;
    public Vector3D<float> Specular = default;

    public DirectionalLightComponent()
    {
    }
}
