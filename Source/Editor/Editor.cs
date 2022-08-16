using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Editor.Host;

namespace Editor;

public class Editor : ApplicationBase
{
    public Editor(bool isEditor) : base(isEditor)
    {
    }

    protected override void InitializeApp()
    {
        base.InitializeApp();

        GetModule<IContentModule>().ContentRootDirectory = "/home/jolly_samurai/Projects/chicken-with-lips/duck/Build/Debug/net6.0/Content";
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new EditorClientHostModule(this, GetModule<ILogModule>()));
    }
}
