using Duck.GameFramework;
using Duck.Logging;
using Editor.Host;

namespace Editor;

public class Editor : ApplicationBase
{
    public Editor(bool isEditor) : base(isEditor)
    {
    }

    protected override void RegisterModules()
    {
        base.RegisterModules();

        AddModule(new EditorClientHostModule(this, GetModule<ILogModule>()));
    }
}
