using System.Diagnostics;
using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.OpenGL;

public abstract class OpenGLRenderObjectBase : IRenderObject
{
    #region Properties

    public abstract uint Id { get; }

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

    public IRenderObject SetParameter(string name, Matrix4X4<float> value)
    {
        _parameters[name] = new Matrix4X4ParameterValue {
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

    public virtual Matrix4X4<float> GetParameterMatrix4X4(string name)
    {
        if (_parameters[name] is Matrix4X4ParameterValue matValue) {
            return matValue.Value;
        }

        throw new Exception("FIXME: errors");
    }

    public bool HasParameter(string name)
    {
        return _parameters.ContainsKey(name);
    }

    #endregion

    private interface IParameterValue
    {
    }

    private struct Matrix4X4ParameterValue : IParameterValue
    {
        public Matrix4X4<float> Value;
    }
}
