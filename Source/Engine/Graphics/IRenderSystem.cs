using Duck.Content;
using Duck.Graphics.Device;
using Duck.Graphics.Shaders;
using Duck.Platform;

namespace Duck.Graphics;

public interface IRenderSystem
{
    public IGraphicsDevice? GraphicsDevice { get; }
    public IAsset<ShaderProgram> DefaultShader { get; }

    public void Init(IApplication app, IWindow window);

    public void PreRender();

    public void Render();

    public void PostRender();
}
