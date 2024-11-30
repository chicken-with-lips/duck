using Duck.Serialization;

namespace Duck.Graphics.Components;

[DuckSerializable]
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
