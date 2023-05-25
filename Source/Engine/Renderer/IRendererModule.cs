using Duck.Renderer.Device;

namespace Duck.Renderer;

public interface IRendererModule : IModule
{
    public IGraphicsDevice? GraphicsDevice { get; }
    public IRenderSystem RenderSystem { get; }
    public View? GameView { get; set; }
    public View[] Views { get; }

    public View CreateView(string name);
    public IScene CreateScene(string name);
}
