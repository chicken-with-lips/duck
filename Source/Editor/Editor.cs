using System.IO;
using System.Reflection;
using ChickenWithLips.WickedEngine;
using Duck.Content;
using Duck.GameFramework;
using Duck.Logging;
using Editor.Host;

namespace Editor;

public class Editor : ApplicationBase
{
    private Application app;
    public Editor(bool isEditor) : base(isEditor)
    {
    }

    protected override void InitializeApp()
    {
        base.InitializeApp();

        // GetModule<IContentModule>().ContentRootDirectory = "/media/jolly_samurai/Data/Projects/chicken-with-lips/duck/Build/Debug/net6.0/Content";
        GetModule<IContentModule>().ContentRootDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),  "Content");
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new EditorClientHostModule(this, GetModule<ILogModule>()));
    }
}
