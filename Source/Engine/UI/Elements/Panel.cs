using System.Drawing;
using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct PanelProps(in Box Box, in Color BackgroundColor)
{
    public static readonly PanelProps Default = new(Box.Default, Color.White);
}

public record struct Panel(PanelProps Props, in Fragment? Child);

public class PanelRenderer : ElementRendererBase, IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<Panel>();

        renderList.DrawBox(
            Measure.ElementPosition(renderContext, e.Props.Box),
            e.Props.Box,
            e.Props.BackgroundColor
        );

        RenderChildrenVertical(
            fragment,
            Vector2D<float>.Zero,
            renderContext,
            renderList,
            e.Child
        );
    }
}

public class PanelPropertyAccessor : IBoxAccessor
{
    public Box GetBox(in Fragment fragment)
    {
        var props = fragment.GetElementAs<Panel>().Props;
        return props.Box;
    }
}

public class PanelFactory : IElementFactory
{
    private readonly ElementPool<Panel> _pool = new();
    private readonly IElementRenderer _elementRenderer = new PanelRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new PanelPropertyAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in PanelProps props, in Fragment? child = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;
        element.Child = child;

        return Fragment.From(ref element, _elementRenderer, _propertyAccessor);
    }
}

public static class PanelExtensions
{
    public static Fragment Panel(this Context context, in PanelProps props, in Fragment? child = null)
    {
        return context.GetFactory<PanelFactory, Panel>().Create(props, child);
    }
}
