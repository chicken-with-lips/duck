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
    public Editor(IPlatform platform, IRenderSystem renderSystem, bool isEditor)
        : base(platform, renderSystem, isEditor)
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
