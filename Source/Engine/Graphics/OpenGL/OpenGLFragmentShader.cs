using Duck.Content;
using Duck.Graphics.Shaders;

namespace Duck.Graphics.OpenGL;

internal class OpenGLFragmentShader : IPlatformAsset<FragmentShader>
{
    public uint ShaderId { get; }

    public OpenGLFragmentShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
