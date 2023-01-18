using Duck.Content;
using Duck.Graphics.Shaders;

namespace Duck.Graphics.OpenGL;

internal class OpenGLShaderProgram : IPlatformAsset<ShaderProgram>
{
    public uint ProgramId { get; }

    public OpenGLShaderProgram(uint programId)
    {
        ProgramId = programId;
    }
}
