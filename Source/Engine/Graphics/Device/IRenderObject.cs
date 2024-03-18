using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Materials;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IRenderObject : IDisposable
{
    public const int MaxTextureSlots = 8;

    public bool IsDisposed { get; }

    public uint Id { get; }
    public uint VertexCount { get; }
    public uint IndexCount { get; }

    public IBoundingVolume BoundingVolume { get; set; }
    public Projection Projection { get; set; }
    public RenderStateFlag RenderStateFlags { get; set; }

    public IRenderObject SetTexture(uint slot, IPlatformAsset<Texture2D> texture);
    public IRenderObject SetMaterial(IPlatformAsset<Material> material);
    public IRenderObject SetParameter(string name, Vector3D<float> value);
    public IRenderObject SetParameter(string name, Matrix4X4<float> value);

    public IPlatformAsset<Texture2D>? GetTexture(uint slot);
    public IPlatformAsset<Material>? GetMaterial();

    public TParameterType GetParameter<TParameterType>(string name)
        where TParameterType : unmanaged;

    public bool HasParameter(string name);

    public IRenderObjectInstance CreateInstance();
}

public interface IRenderObjectInstance : IRenderObject
{
    public uint ParentId { get; }
}

public enum Projection
{
    Perspective,
    Orthographic
}

[Flags]
public enum RenderStateFlag
{
    None,
    DisableDepthTesting
}
