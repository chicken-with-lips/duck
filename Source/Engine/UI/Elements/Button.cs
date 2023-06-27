namespace Duck.Ui.Elements;

public readonly record struct ButtonProps(in string Label)
{
    public static readonly ButtonProps Default = new("Test");
}

public record struct Button(in ButtonProps Props);

public class ButtonFactory : IElementFactory
{
    private readonly ElementPool<Button> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new ButtonRenderer();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in ButtonProps props)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;

        return Fragment.From(ref element, _defaultElementRenderer);
    }
}

public class ButtonRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        // Console.WriteLine(fragment2.GetElementAs<Button>().Props.Label);
    }
}

public static class ButtonExtensions
{
    public static Fragment Button(this Context context, in ButtonProps props)
    {
        return context.GetFactory<ButtonFactory, Button>().Create(props);
    }
}
