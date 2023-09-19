using Arch.Core;
using Duck.Renderer.Device;

namespace Duck.Renderer;

public interface IRendererModule : IModule
{
    public IGraphicsDevice GraphicsDevice { get; }
    public IRenderSystem RenderSystem { get; }
    public View PrimaryView { get; set; }
    
    public IScene[] Scenes { get; }

    public View CreateView(string name);
    public IScene CreateScene(string name, World? world = null);

    public View? FindViewForScene(IScene scene);
    public IScene? FindScene(string name);

    public IScene GetOrCreateScene(string name);
    public void UnloadScene(IScene scene);
}
