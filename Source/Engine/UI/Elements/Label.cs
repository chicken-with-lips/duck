namespace Duck.Ui.Elements;

public readonly record struct LabelProps(in string Content)
{
    public static readonly LabelProps Default = new(string.Empty);
}

public record struct Label(in LabelProps Props);

public class LabelFactory : IElementFactory
{
    private readonly ElementPool<Label> _pool = new();
    private readonly IElementRenderer _defaultRenderer = new LabelRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new LabelPropertyAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in LabelProps props)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;

        return Fragment.From(ref element, _defaultRenderer, _propertyAccessor);
    }
}

public class LabelPropertyAccessor : IBoxAccessor
{
    public Box GetBox(in Fragment fragment)
    {
        return new Box();
    }
}

public class LabelRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<Label>();
        
        if (renderContext.Font.HasValue) {
            renderList.DrawText(renderContext.Font, e.Props.Content, renderContext.Position);
        }
    }
}

public static class LabelExtensions
{
    public static Fragment Label(this Context context, in LabelProps props)
    {
        return context.GetFactory<LabelFactory, Label>().Create(props);
    }
}
