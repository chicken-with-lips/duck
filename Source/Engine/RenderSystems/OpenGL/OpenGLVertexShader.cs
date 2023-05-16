using Duck.Content;
using Duck.Renderer.Shaders;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLVertexShader : PlatformAssetBase<VertexShader>
{
    public uint ShaderId { get; internal set; }

    public OpenGLVertexShader(uint shaderId)
    {
        ShaderId = shaderId;
    }
}
