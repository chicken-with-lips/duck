using System.Drawing;
using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct ButtonProps(in Box Box, in Color BackgroundColor, in Color HoverColor)
{
    public static readonly ButtonProps Default = new(Box.Default, Color.White, Color.White);
}

public record struct Button(ButtonProps Props, in Fragment? Child, Action? OnClicked = null);

public class ButtonRenderer : ElementRendererBase, IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<Button>();
        var backgroundColor = e.Props.BackgroundColor;

        if (Measure.IsPointInside(renderContext, fragment, renderContext.Input.GetMousePosition(0))) {
            backgroundColor = e.Props.HoverColor;

            if (!renderContext.Input.WasMouseButtonDown(0) && renderContext.Input.IsMouseButtonDown(0)) {
                e.OnClicked?.Invoke();
            }
        }

        renderList.DrawBox(
            Measure.ElementPosition(renderContext, e.Props.Box),
            e.Props.Box,
            backgroundColor
        );

        RenderChildrenVertical(
            fragment,
            Vector2D<float>.Zero,
            renderContext,
            renderList,
            fragment.GetElementAs<Button>().Child
        );
    }
}

public class ButtonPropertyAccessor : IBoxAccessor
{
    public Box GetBox(in Fragment fragment)
    {
        return fragment.GetElementAs<Button>().Props.Box;
    }
}

public class ButtonFactory : IElementFactory
{
    private readonly ElementPool<Button> _pool = new();
    private readonly IElementRenderer _elementRenderer = new ButtonRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new ButtonPropertyAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in ButtonProps props, in Fragment? child = null, Action? onClicked = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;
        element.Child = child;
        element.OnClicked = onClicked;

        return Fragment.From(ref element, _elementRenderer, _propertyAccessor);
    }
}

public static class ButtonExtensions
{
    public static Fragment Button(this Context context, in ButtonProps props, in Fragment? child = null, Action? onClicked = null)
    {
        return context.GetFactory<ButtonFactory, Button>().Create(props, child, onClicked);
    }
}
