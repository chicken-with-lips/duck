using System.Collections;

namespace Duck.Content;

public interface IMaterialInstanceAsset : IAsset
{
    public AssetReference<IMaterialAsset> MaterialAssetReference { get; }
    public MaterialParameterCollection Parameters { get; }

    public void SetParameter(string name, AssetReference<ITexture2DAsset> texture);
    // public void SetParameter(string name, RgbType rgbType, Color color);
    // public void SetParameter(string name, RgbaType rgbaType, Color color);
}

public interface IMaterialInstance
{
    public IMaterialInstanceAsset Asset { get; }
}

public class MaterialParameterCollection : IEnumerable
{
    private readonly Dictionary<string, IParameter> _properties = new();

    public void SetParameter(string name, AssetReference<ITexture2DAsset> texture)
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
        public AssetReference<ITexture2DAsset> Value;
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
