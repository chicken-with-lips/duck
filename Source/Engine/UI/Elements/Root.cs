using Duck.Content;
using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct RootProps(in Box Box, in AssetReference<Font>? Font)
{
    public static readonly RootProps Default = new(Box.Default, null);
}

public record struct Root(in RootProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class RootFactory : IElementFactory
{
    public Span<Root> Roots => _pool.ToSpan();

    private readonly ElementPool<Root> _pool = new();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public ref Root Create(RootProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;
        element.Child0 = child0;
        element.Child1 = child1;
        element.Child2 = child2;
        element.Child3 = child3;
        element.Child4 = child4;
        element.Child5 = child5;

        return ref element;
    }
}

public class RootRenderer
{
    public void Render(ref Root root, in ElementRenderContext renderContext, RenderList renderList)
    {
        var offsetInPixels = renderContext.Position
                             + Measure.ConvertEmToPixels(new Vector2D<float>(root.Props.Box.Padding.Left, root.Props.Box.Padding.Top));

        if (root.Child0.HasValue) {
            root.Child0.Value.ElementRenderer.Render(root.Child0.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = root.Props.Box,
                BoxInPixels = Measure.ConvertEmToPixels(root.Props.Box),
                Font = root.Props.Font,
                ParentBox = renderContext.Box,
                ParentBoxInPixels = renderContext.BoxInPixels,
            }, renderList);
        }

        if (root.Child1.HasValue) {
            throw new Exception("TODO: block level");
            root.Child1.Value.ElementRenderer.Render(root.Child1.Value, ElementRenderContext.Default, renderList);
        }

        if (root.Child2.HasValue) {
            throw new Exception("TODO: block level");
            root.Child2.Value.ElementRenderer.Render(root.Child2.Value, ElementRenderContext.Default, renderList);
        }

        if (root.Child3.HasValue) {
            throw new Exception("TODO: block level");
            root.Child3.Value.ElementRenderer.Render(root.Child3.Value, ElementRenderContext.Default, renderList);
        }

        if (root.Child4.HasValue) {
            throw new Exception("TODO: block level");
            root.Child4.Value.ElementRenderer.Render(root.Child4.Value, ElementRenderContext.Default, renderList);
        }

        if (root.Child5.HasValue) {
            throw new Exception("TODO: block level");
            root.Child5.Value.ElementRenderer.Render(root.Child5.Value, ElementRenderContext.Default, renderList);
        }
    }
}
