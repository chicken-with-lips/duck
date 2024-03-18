using Duck.Graphics;
using Duck.Platform;

namespace Duck.Ui;

public interface IUIModule : IModule
{
    public Context GetContextForScene(IScene scene);
}
