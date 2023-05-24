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
    public DirectionalLight[] DirectionalLights => _directionalLights.ToArray();

    private readonly List<IRenderObject> _renderables = new();
    private readonly List<DirectionalLight> _directionalLights = new List<DirectionalLight>();

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

    public void AddDirectionalLight(Vector3D<float> direction, Vector3D<float> ambient, Vector3D<float> diffuse, Vector3D<float> specular)
    {
        _directionalLights.Add(
            new DirectionalLight() {
                Direction = direction,
                Ambient = ambient,
                Diffuse = diffuse,
                Specular = specular
            }
        );
    }

    public struct DirectionalLight
    {
        public Vector3D<float> Direction;
        public Vector3D<float> Ambient;
        public Vector3D<float> Diffuse;
        public Vector3D<float> Specular;
    }
}
