using System.IO;
using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Duck.Platform;
using Duck.Graphics;
using GameLauncher.Host;

namespace GameLauncher;

public class LauncherApp : ApplicationBase
{
    private readonly string _projectDirectory;

    public LauncherApp(IPlatform platform, IRenderSystem renderSystem, string projectDirectory)
        : base(platform, renderSystem, false)
    {
        _projectDirectory = projectDirectory;
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        GetModule<IContentModule>().ContentRootDirectory = Path.Combine(_projectDirectory, "Content");
        GetModule<IContentModule>().ReloadChangedContent = false;

        AddModule(new GameClientHostModule(this, GetModule<ILogModule>(), _projectDirectory));
    }
}
