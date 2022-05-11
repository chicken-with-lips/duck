using Duck.Content;
using Duck.Graphics.Shaders;

namespace Duck.Graphics.OpenGL;

public class OpenGLVertexShader : IPlatformAsset<VertexShader>
{
    public uint ShaderId { get; }

    public OpenGLVertexShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
