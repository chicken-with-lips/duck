using Duck.GameFramework;
using Duck.Logging;
using Editor.Host;

namespace Editor;

public class Editor : ApplicationBase
{
    public Editor(bool isEditor) : base(isEditor)
    {
    }

    protected override void RegisterSubsystems()
    {
        base.RegisterSubsystems();

        AddSubsystem(new EditorClientHostSubsystem(this, GetSubsystem<ILogSubsystem>()));
    }
}
