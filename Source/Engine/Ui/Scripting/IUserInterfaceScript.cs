namespace Duck.Ui.Scripting;

public interface IUserInterfaceScript
{
}

public interface IUserInterfaceLoaded : IUserInterfaceScript
{
    public void OnLoaded();
}
