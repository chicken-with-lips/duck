using System.IO;
using System.Reflection;
using Duck.Content;
using Duck.GameFramework;
using Duck.Graphics;
using Duck.Logging;
using Duck.Platform;
using Editor.Host;

namespace Editor;

public class Editor : ApplicationBase
{
    private readonly string _projectDirectory;

    public Editor(IPlatform platform, IRenderSystem renderSystem, string projectDirectory)
        : base(platform, renderSystem, true)
    {
        _projectDirectory = projectDirectory;
    }

    protected override void InitializeApp()
    {
        base.InitializeApp();

        GetModule<IContentModule>().ContentRootDirectory = Path.Combine(_projectDirectory, "Content");
        GetModule<IContentModule>().ReloadChangedContent = true;
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new EditorClientHostModule(this, GetModule<ILogModule>(), _projectDirectory));
    }
}
