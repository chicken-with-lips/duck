using Duck.Content;
using Duck.Renderer.Device;
using Duck.Renderer.Mesh;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLStaticMesh : PlatformAssetBase<StaticMesh>, IRenderable
{
    public IRenderObject RenderObject { get; internal set;  }

    public OpenGLStaticMesh(IRenderObject renderObject)
    {
        RenderObject = renderObject;
    }
}
