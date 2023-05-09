using Duck.Graphics.Device;

namespace Duck.Graphics;

public interface IGraphicsModule : IModule
{
    public IGraphicsDevice? GraphicsDevice { get; }
    public IRenderSystem RenderSystem { get; }
}
