using Duck.Content;
using Duck.Renderer.Shaders;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLFragmentShader : PlatformAssetBase<FragmentShader>
{
    public uint ShaderId { get; internal set; }

    public OpenGLFragmentShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
