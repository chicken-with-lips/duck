using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public interface IElementPropertyAccessor
{
}

public interface IBoxAccessor : IElementPropertyAccessor
{
    public Box GetBox(in Fragment fragment);
}

public interface IContentAccessor : IElementPropertyAccessor
{
    public Vector2D<float> GetContentDimensions(in Fragment fragment);
}
