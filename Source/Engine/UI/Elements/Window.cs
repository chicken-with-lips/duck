namespace Duck.Ui.Elements;

public record struct WindowProps(in string Title, in int Width, in int Height)
{
    public static readonly WindowProps Default = new("Window", 100, 100);
}

public record struct Window(in WindowProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class WindowRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        // Console.WriteLine(fragment2.GetElementAs<Window2>().Props.Title);
    }
}

public class WindowFactory : IElementFactory
{
    private readonly ElementPool<Window> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new WindowRenderer();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in WindowProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;
        element.Child0 = child0;
        element.Child1 = child1;
        element.Child2 = child2;
        element.Child3 = child3;
        element.Child4 = child4;
        element.Child5 = child5;

        return Fragment.From(ref element, _defaultElementRenderer);
    }
}

public static class WindowExtensions
{
    public static Fragment Window(this Context context, in WindowProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        return context.GetFactory<WindowFactory, Window>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
