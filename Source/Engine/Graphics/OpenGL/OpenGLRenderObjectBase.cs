using System.Diagnostics;
using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.OpenGL;

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

    private IPlatformAsset<ShaderProgram>? _shaderProgram;

    #endregion

    #region Methods

    public IRenderObject SetTexture(uint slot, IPlatformAsset<Texture2D> texture)
    {
        Debug.Assert(slot < IRenderObject.MaxTextureSlots);

        _textures[slot] = texture;

        return this;
    }

    public IRenderObject SetShaderProgram(IPlatformAsset<ShaderProgram> program)
    {
        _shaderProgram = program;

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

    public virtual IPlatformAsset<ShaderProgram>? GetShaderProgram()
    {
        return _shaderProgram;
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
