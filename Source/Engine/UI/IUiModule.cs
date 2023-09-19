using Duck.Renderer;

namespace Duck.Ui;

public interface IUiModule : IModule
{
    public Context GetContextForScene(IScene scene);
}
