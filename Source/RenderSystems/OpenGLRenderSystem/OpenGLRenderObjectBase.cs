using System.Diagnostics;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Materials;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.RenderSystems.OpenGL;

internal abstract class OpenGLRenderObjectBase : IRenderObject
{
    #region Properties

    public abstract bool IsDisposed { get; }

    public abstract uint Id { get; }
    public abstract uint VertexCount { get; }
    public abstract uint IndexCount { get; }

    public virtual Projection Projection { get; set; }
    public virtual IBoundingVolume BoundingVolume { get; set; }
    public RenderStateFlag RenderStateFlags { get; set; } = RenderStateFlag.None;

    #endregion

    #region Members

    private readonly IPlatformAsset<Texture2D>[] _textures = new IPlatformAsset<Texture2D>[IRenderObject.MaxTextureSlots];
    private readonly Dictionary<string, IParameterValue> _parameters = new();

    private IPlatformAsset<Material>? _material;

    #endregion

    #region Methods

    public IRenderObject SetTexture(uint slot, IPlatformAsset<Texture2D> texture)
    {
        Debug.Assert(slot < IRenderObject.MaxTextureSlots);

        _textures[slot] = texture;

        return this;
    }

    public IRenderObject SetMaterial(IPlatformAsset<Material> material)
    {
        _material = material;

        return this;
    }

    public IRenderObject SetParameter(string name, Vector3D<float> value)
    {
        _parameters[name] = new TypedParameterValue<Vector3D<float>>() {
            Value = value
        };

        return this;
    }

    public IRenderObject SetParameter(string name, Matrix4X4<float> value)
    {
        _parameters[name] = new TypedParameterValue<Matrix4X4<float>>() {
            Value = value
        };

        return this;
    }

    public virtual IPlatformAsset<Texture2D>? GetTexture(uint slot)
    {
        Debug.Assert(slot < IRenderObject.MaxTextureSlots);

        return _textures[slot];
    }

    public virtual IPlatformAsset<Material>? GetMaterial()
    {
        return _material;
    }

    public virtual TParameterType GetParameter<TParameterType>(string name)
        where TParameterType : unmanaged
    {
        return ((IParameterValue<TParameterType>)_parameters[name]).GetValue();
    }

    public bool HasParameter(string name)
    {
        return _parameters.ContainsKey(name);
    }

    public abstract IRenderObjectInstance CreateInstance();
    
    public abstract void Dispose();

    #endregion

    private interface IParameterValue
    {
    }

    private interface IParameterValue<TDataType> : IParameterValue
        where TDataType : unmanaged
    {
        public TDataType GetValue();
    }

    private struct TypedParameterValue<TDataType> : IParameterValue<TDataType>
        where TDataType : unmanaged
    {
        public TDataType Value;

        public TDataType GetValue()
        {
            return Value;
        }
    }
}
