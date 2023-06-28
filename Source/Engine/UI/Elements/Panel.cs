using System.Drawing;
using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct PanelProps(in Box Box, in Color BackgroundColor)
{
    public static readonly PanelProps Default = new(Box.Default, Color.White);
}

public record struct Panel(PanelProps Props, in Fragment? Child);

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

public class PanelPropertyAccessor : IBoxAccessor
{
    public Box GetBox(in Fragment fragment)
    {
        var props = fragment.GetElementAs<Panel>().Props;
        return props.Box;
    }
}

public class PanelRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        renderList.DrawBox(renderContext.Position, Measure.ConvertEmToPixels(renderContext.Box.ToVector()), fragment.GetElementAs<Panel>().Props.BackgroundColor);

        var e = fragment.GetElementAs<Panel>();
        var offsetInPixels = renderContext.Position
                             + Measure.ConvertEmToPixels(new Vector2D<float>(e.Props.Box.Padding.Left, e.Props.Box.Padding.Top));

        if (e.Child is { PropertyAccessor: IBoxAccessor accessor0 }) {
            var box = accessor0.GetBox(e.Child.Value);

            e.Child.Value.ElementRenderer.Render(e.Child.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = box,
                Font = renderContext.Font
            }, renderList);
        }
    }
}

public static class PanelExtensions
{
    public static Fragment Panel(this Context context, in PanelProps props, in Fragment? child = null)
    {
        return context.GetFactory<PanelFactory, Panel>().Create(props, child);
    }
}
