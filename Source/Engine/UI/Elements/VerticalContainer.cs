using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct VerticalContainerProps(in Box Box, in float GapSize, in VerticalAlign VerticalAlignment)
{
    public static readonly VerticalContainerProps Default = new(Box.Default, 1, VerticalAlign.Top);
}

public record struct VerticalContainer(in VerticalContainerProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class VerticalContainerRenderer : ElementRendererBase, IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<VerticalContainer>();
        var offset = Measure.ElementPosition(renderContext, e.Props.Box);
        var gapSize = new Vector2D<float>(0, e.Props.GapSize);

        if (e.Props.VerticalAlignment != VerticalAlign.Top) {
            var contentDimensions = Measure.ContentDimensions(fragment);
            var containerHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(e.Props.Box);

            switch (e.Props.VerticalAlignment) {
                case VerticalAlign.Center:
                    offset += new Vector2D<float>(0, (containerHeight / 2f) - (contentDimensions.Y / 2f));
                    break;

                case VerticalAlign.Bottom:
                    offset += new Vector2D<float>(0, containerHeight - contentDimensions.Y);
                    break;
            }
        }

        RenderChildrenVertical(
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

public class VerticalContainerAccessor : IContentAccessor
{
    public Vector2D<float> GetContentDimensions(in Fragment fragment)
    {
        var e = fragment.GetElementAs<VerticalContainer>();

        return Measure.ContentDimensionsVertical(
            BoxArea.Default with {
                Top = e.Props.GapSize
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

public class VerticalContainerFactory : IElementFactory
{
    private readonly ElementPool<VerticalContainer> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new VerticalContainerRenderer();
    private readonly IElementPropertyAccessor _propertyAccessor = new VerticalContainerAccessor();

    public void BeginFrame()
    {
        _pool.ResetIndex();
    }

    public Fragment Create(in VerticalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
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

public static class VerticalContainerExtensions
{
    public static Fragment VerticalContainer(this Context context, in VerticalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        return context.GetFactory<VerticalContainerFactory, VerticalContainer>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
