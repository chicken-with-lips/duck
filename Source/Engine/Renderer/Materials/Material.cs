using System.Collections;
using Duck.Content;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
using Silk.NET.Maths;

namespace Duck.Renderer.Materials;

public class Material : AssetBase<Material>
{
    #region Properties

    public AssetReference<Texture2D>? DiffuseTexture { get; set; }
    public Vector3D<float> Specular { get; set; } = Vector3D<float>.Zero;
    public float Shininess { get; set; } = 32f;

    public AssetReference<ShaderProgram>? Shader { get; set; }

    #endregion

    #region Methods

    public Material(AssetImportData importData)
        : base(importData)
    {
    }

    #endregion
}

public class MaterialParameterCollection : IEnumerable
{
    private readonly Dictionary<string, IParameter> _properties = new();

    public void SetParameter(string name, AssetReference<Texture2D> texture)
    {
        _properties.Add(name, new Texture2DParameter() {
            Name = name,
            Value = texture,
        });
    }

    // public void SetParameter(string name, RgbType rgbType, Color color)
    // {
    //     _properties.Add(name, new RgbColor() {
    //         Name = name,
    //         Value = color,
    //     });
    // }
    //
    // public void SetParameter(string name, RgbaType rgbaType, Color color)
    // {
    //     _properties.Add(name, new RgbaColor() {
    //         Name = name,
    //         Value = color,
    //     });
    // }

    public IEnumerator GetEnumerator()
    {
        return _properties.GetEnumerator();
    }

    public interface IParameter
    {
    }

    public struct Texture2DParameter : IParameter
    {
        public string Name;
        public AssetReference<Texture2D> Value;
    }

    public struct RgbColor : IParameter
    {
        public string Name;
        // public RgbType Type;
        // public Color Value;
    }

    public struct RgbaColor : IParameter
    {
        public string Name;
        // public RgbaType Type;
        // public Color Value;
    }
}
