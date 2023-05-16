using Duck.Content;
using Duck.Renderer.Shaders;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLShaderProgram : PlatformAssetBase<ShaderProgram>
{
    public uint ProgramId { get; internal set; }
    public OpenGLFragmentShader FragmentShader { get; }
    public OpenGLVertexShader VertexShader { get; }

    public OpenGLShaderProgram(uint programId, OpenGLVertexShader vertexShader, OpenGLFragmentShader fragmentShader)
    {
        ProgramId = programId;
        FragmentShader = fragmentShader;
        VertexShader = vertexShader;
    }
}
