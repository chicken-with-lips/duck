using Duck.Content;
using Duck.Graphics.Shaders;

namespace Duck.Graphics.OpenGL;

internal class OpenGLVertexShader : IPlatformAsset<VertexShader>
{
    public uint ShaderId { get; }

    public OpenGLVertexShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
