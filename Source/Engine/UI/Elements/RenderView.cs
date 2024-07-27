using Duck.Graphics;

namespace Duck.Ui.Elements;

public readonly record struct RenderViewProps(in Box Box, in View? View)
{
    public static readonly RenderViewProps Default = new(Box.Default, null);
}

public record struct RenderView(RenderViewProps Props);

public class RenderViewRenderer : ElementRendererBase, IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<RenderView>();

        if (e.Props.View == null) {
            return;
        }
        
        var position = Measure.ElementPositionInPixels(renderContext, e.Props.Box);
        var dimensions = Measure.BoxDimensionsInPixels(fragment);

        e.Props.View.AutoSizeToWindow = false;
        e.Props.View.Position = position.As<int>();
        e.Props.View.Dimensions = dimensions.As<int>();
    }
}

public class RenderViewPropertyAccessor : IBoxAccessor
{
    public Box GetBox(in Fragment fragment)
    {
        var props = fragment.GetElementAs<RenderView>().Props;
        return props.Box;
    }
}

public class RenderViewFactory : IElementFactory
{
    private readonly ElementPool<RenderView> _pool = new();
    private readonly IElementRenderer _elementRenderer = new RenderViewRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new RenderViewPropertyAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in RenderViewProps props, in Fragment? child = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;

        return Fragment.From(ref element, _elementRenderer, _propertyAccessor);
    }
}

public static class RenderViewExtensions
{
    public static Fragment RenderView(this Context context, in RenderViewProps props, in Fragment? child = null)
    {
        return context.GetFactory<RenderViewFactory, RenderView>().Create(props, child);
    }
}
