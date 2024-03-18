using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;

namespace Duck.RenderSystems.OpenGL;

internal class OpenGLStaticMesh : PlatformAssetBase<StaticMesh>, IRenderable
{
    public IRenderObject RenderObject { get; internal set;  }

    public OpenGLStaticMesh(IRenderObject renderObject)
    {
        RenderObject = renderObject;
    }
}
