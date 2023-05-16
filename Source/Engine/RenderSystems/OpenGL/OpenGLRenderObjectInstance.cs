using Duck.Content;
using Duck.Renderer.Components;
using Duck.Renderer.Device;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
using Silk.NET.Maths;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLRenderObjectInstance : OpenGLRenderObjectBase, IRenderObjectInstance
{
    #region Properties

    public override uint Id => _id;
    public uint ParentId => _renderObject.Id;
    public override uint VertexCount => _renderObject.VertexCount;
    public override uint IndexCount => _renderObject.IndexCount;

    public override Projection Projection {
        get => _renderObject.Projection;
        set => throw new NotSupportedException();
    }

    public override IBoundingVolume BoundingVolume {
        get => _localBoundingVolume ?? _renderObject.BoundingVolume;
        set => _localBoundingVolume = value;
    }

    #endregion

    #region Members

    private readonly uint _id;
    private readonly IRenderObject _renderObject;
    private IBoundingVolume? _localBoundingVolume;

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

    public override IPlatformAsset<Material>? GetMaterial()
    {
        return base.GetMaterial() ?? _renderObject.GetMaterial();
    }

    public override TParameterType GetParameter<TParameterType>(string name)
    {
        return HasParameter(name) ? base.GetParameter<TParameterType>(name) : _renderObject.GetParameter<TParameterType>(name);
    }

    public override IRenderObjectInstance CreateInstance()
    {
        throw new Exception("This should be called on a RenderObject not an instance");
    }

    public override bool IsDisposed { get; }

    public override void Dispose()
    {
    }

    #endregion
}
