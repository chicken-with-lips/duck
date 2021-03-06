using System.Collections;
using Duck.Content;
using Duck.Graphics.Textures;

namespace Duck.Graphics.Materials;

public class Material : AssetBase<Material>
{
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
