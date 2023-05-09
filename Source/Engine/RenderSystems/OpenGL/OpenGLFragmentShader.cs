using Duck.Content;
using Duck.Graphics.Shaders;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLFragmentShader : PlatformAssetBase<FragmentShader>
{
    public uint ShaderId { get; internal set; }

    public OpenGLFragmentShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
