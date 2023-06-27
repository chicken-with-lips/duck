using Duck.Content;
using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public interface IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList);
}

public readonly record struct ElementRenderContext(in Vector2D<float> Position, in Box Box, in Box BoxInPixels, in Box ParentBox, in Box ParentBoxInPixels, in AssetReference<Font>? Font)
{
    public static readonly ElementRenderContext Default = new();
}
