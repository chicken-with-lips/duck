using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Textures;
using Duck.Math;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLStaticMesh : IPlatformAsset<StaticMesh>, IRenderable
{
    public IRenderObject RenderObject { get; }

    public OpenGLStaticMesh(IRenderObject renderObject)
    {
        RenderObject = renderObject;
    }
}
