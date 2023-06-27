using Duck.Ui.Elements;

namespace Duck.Ui;

public interface IElementPropertyAccessor
{
}

public interface IBoxAccessor : IElementPropertyAccessor
{
    public Box GetBox(in Fragment fragment);
}
