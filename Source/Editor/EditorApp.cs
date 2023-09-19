using System.IO;
using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Duck.Platform;
using Duck.Renderer;
using Editor.Modules;

namespace Editor;

public class EditorApp : ApplicationBase
{
    private readonly string _projectDirectory;

    public EditorApp(IPlatform platform, IRenderSystem renderSystem, string projectDirectory)
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

        AddModule(new EditorModule(this, GetModule<ILogModule>(), GetModule<IRendererModule>(), GetModule<IContentModule>(), _projectDirectory));
        AddModule(new GameHostModule(this, GetModule<ILogModule>(), _projectDirectory));
    }
}
