using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.OpenGL;

public class OpenGLRenderObjectInstance : OpenGLRenderObjectBase, IRenderObjectInstance
{
    #region Properties

    public override uint Id => _id;
    public uint ParentId => _renderObject.Id;

    
    #endregion

    #region Members

    private readonly uint _id;
    private readonly IRenderObject _renderObject;

    #endregion

    #region Methods

    internal OpenGLRenderObjectInstance(uint id, IRenderObject renderObject)
    {
        _id = id;
        _renderObject = renderObject;
    }

    public override IPlatformAsset<Texture2D>? GetTexture(uint slot)
    {
        return base.GetTexture(slot) ?? _renderObject.GetTexture(slot);
    }

    public override IPlatformAsset<ShaderProgram>? GetShaderProgram()
    {
        return base.GetShaderProgram() ?? _renderObject.GetShaderProgram();
    }

    public override Matrix4X4<float> GetParameterMatrix4X4(string name)
    {
        return HasParameter(name) ? base.GetParameterMatrix4X4(name) : _renderObject.GetParameterMatrix4X4(name);
    }

    #endregion
}
