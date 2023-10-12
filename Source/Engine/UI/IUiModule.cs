using Duck.Graphics;
using Duck.Platform;

namespace Duck.Ui;

public interface IUiModule : IModule
{
    public Context GetContextForScene(IScene scene);
}
