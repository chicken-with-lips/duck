using Duck.Ui.RmlUi;

namespace Duck.Ui.Scripting;

public interface IUserInterfaceScript
{
}

public interface IUserInterfaceLoaded : IUserInterfaceScript
{
    public void OnLoaded(RmlUserInterface ui);
}

public interface IUserInterfaceTick : IUserInterfaceScript
{
    public void OnTick();
}
