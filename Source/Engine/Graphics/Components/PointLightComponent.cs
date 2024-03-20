using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Graphics.Components;

[AutoSerializable]
public partial struct PointLightComponent
{
    public AVector3 Ambient = default;
    public AVector3 Diffuse = default;
    public AVector3 Specular = default;

    public float Constant = default;
    public float Linear = default;
    public float Quadratic = default;

    public PointLightComponent()
    {
    }
}
