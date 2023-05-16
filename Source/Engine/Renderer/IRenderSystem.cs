using Duck.Content;
using Duck.Renderer.Device;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;
using Duck.Platform;
using Silk.NET.Maths;

namespace Duck.Renderer;

public interface IRenderSystem
{
    public IGraphicsDevice? GraphicsDevice { get; }
    public IAsset<ShaderProgram> FallbackShader { get; }
    public IAsset<Material> FallbackMaterial { get; }

    public void Init(IApplication app, IWindow window);

    public void PreRender();

    public void Render();

    public void PostRender();
}

public class CommandBuffer
{
    public View View { get; }
    public IGraphicsDevice GraphicsDevice { get; }
    public Matrix4X4<float> ViewMatrix { get; set; }

    public IRenderObject[] Renderables => _renderables.ToArray();

    private readonly List<IRenderObject> _renderables = new();

    public CommandBuffer(View view, IGraphicsDevice graphicsDevice)
    {
        View = view;
        GraphicsDevice = graphicsDevice;
    }


    public void ScheduleRenderable(IRenderObject renderObject)
    {
        _renderables.Add(renderObject);
    }

    public void ScheduleRenderableInstance(uint instanceId)
    {
        _renderables.Add(GraphicsDevice.GetRenderObjectInstance(instanceId));
    }
}
