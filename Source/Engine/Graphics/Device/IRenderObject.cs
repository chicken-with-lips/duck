using Duck.Content;
using Duck.Graphics.Components;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IRenderObject : IDisposable
{
    public bool IsDisposed { get; }
    
    public const int MaxTextureSlots = 8;

    public uint Id { get; }
    public uint VertexCount { get; }
    public uint IndexCount { get; }

    public IBoundingVolume BoundingVolume { get; set; }

    public IRenderObject SetTexture(uint slot, IPlatformAsset<Texture2D> texture);
    public IRenderObject SetShaderProgram(IPlatformAsset<ShaderProgram> program);
    public IRenderObject SetParameter(string name, Vector3D<float> value);
    public IRenderObject SetParameter(string name, Matrix4X4<float> value);

    public IPlatformAsset<Texture2D>? GetTexture(uint slot);
    public IPlatformAsset<ShaderProgram>? GetShaderProgram();

    public TParameterType GetParameter<TParameterType>(string name)
        where TParameterType : unmanaged;

    public bool HasParameter(string name);
}

public interface IRenderObjectInstance : IRenderObject
{
    public uint ParentId { get; }
}
