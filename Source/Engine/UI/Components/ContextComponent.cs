using Duck.Serialization;

namespace Duck.Ui.Components;

[AutoSerializable]
public partial struct ContextComponent
{
    public string Name = String.Empty;
    public UiSpace Space = UiSpace.ScreenSpace;
    public UiSizing Sizing = UiSizing.FillX | UiSizing.FillY;
    public bool ShouldReceiveInput = false;
}

public enum UiSpace
{
    ScreenSpace
}

[Flags]
public enum UiSizing
{
    FillX,
    FillY,
}
