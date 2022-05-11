using Duck.Content;
using Duck.Graphics.Shaders;
using Duck.Graphics.Textures;
using Silk.NET.Maths;

namespace Duck.Graphics.Device;

public interface IRenderObject
{
    public const int MaxTextureSlots = 8;

    public uint Id { get; }

    public IRenderObject SetTexture(uint slot, IPlatformAsset<Texture2D> texture);
    public IRenderObject SetShaderProgram(IPlatformAsset<ShaderProgram> program);
    public IRenderObject SetParameter(string name, Matrix4X4<float> value);

    public IPlatformAsset<Texture2D>? GetTexture(uint slot);
    public IPlatformAsset<ShaderProgram>? GetShaderProgram();
    public Matrix4X4<float> GetParameterMatrix4X4(string name);
    public bool HasParameter(string name);
}

public interface IRenderObjectInstance : IRenderObject
{
    public uint Id { get; }
    public uint ParentId { get; }
}
