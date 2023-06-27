using System.IO;
using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Duck.Platform;
using Duck.Renderer;
using GameLauncher.Host;

namespace GameLauncher;

public class Game : ApplicationBase
{
    private readonly string _projectDirectory;

    public Game(IPlatform platform, IRenderSystem renderSystem, string projectDirectory)
        : base(platform, renderSystem, false)
    {
        _projectDirectory = projectDirectory;
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        GetModule<IContentModule>().ContentRootDirectory = Path.Combine(_projectDirectory, "Content");
        GetModule<IContentModule>().ReloadChangedContent = true;

        AddModule(new GameClientHostModule(this, GetModule<ILogModule>(), _projectDirectory));
    }
}
