using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct HorizontalContainerProps(in Box Box, in float GapSize, in HorizontalAlign HorizontalAlignment)
{
    public static readonly HorizontalContainerProps Default = new(Box.Default, 1, HorizontalAlign.Left);
}

public record struct HorizontalContainer(in HorizontalContainerProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class HorizontalContainerRenderer : ElementRendererBase, IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<HorizontalContainer>();
        var offset = Measure.ElementPosition(renderContext, e.Props.Box);
        var gapSize = new Vector2D<float>(e.Props.GapSize, 0);

        if (e.Props.HorizontalAlignment != HorizontalAlign.Left) {
            var contentDimensions = Measure.ContentDimensions(fragment);
            var containerWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(e.Props.Box);

            switch (e.Props.HorizontalAlignment) {
                case HorizontalAlign.Center:
                    offset += new Vector2D<float>((containerWidth / 2f) - (contentDimensions.X / 2f), 0);
                    break;

                case HorizontalAlign.Right:
                    offset += new Vector2D<float>(containerWidth - contentDimensions.X, 0);
                    break;
            }
        }

        RenderChildrenHorizontal(
            offset,
            gapSize,
            renderContext,
            renderList,
            e.Child0,
            e.Child1,
            e.Child2,
            e.Child3,
            e.Child4,
            e.Child5
        );
    }
}

public class HorizontalContainerAccessor : IContentAccessor
{
    public Vector2D<float> GetContentDimensions(in Fragment fragment)
    {
        var e = fragment.GetElementAs<HorizontalContainer>();

        return Measure.ContentDimensionsHorizontal(
            BoxArea.Default with {
                Left = e.Props.GapSize
            },
            e.Child0,
            e.Child1,
            e.Child2,
            e.Child3,
            e.Child4,
            e.Child5
        );
    }
}

public class HorizontalContainerFactory : IElementFactory
{
    private readonly ElementPool<HorizontalContainer> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new HorizontalContainerRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new HorizontalContainerAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in HorizontalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        ref var element = ref _pool.Allocate();
        element.Props = props;
        element.Child0 = child0;
        element.Child1 = child1;
        element.Child2 = child2;
        element.Child3 = child3;
        element.Child4 = child4;
        element.Child5 = child5;

        return Fragment.From(ref element, _defaultElementRenderer, _propertyAccessor);
    }
}

public static class HorizontalContainerExtensions
{
    public static Fragment HorizontalContainer(this Context context, in HorizontalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        return context.GetFactory<HorizontalContainerFactory, HorizontalContainer>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
