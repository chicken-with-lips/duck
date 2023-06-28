using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct VerticalContainerProps(in Box Box, in float GapSize, in VerticalAlign VerticalAlignment)
{
    public static readonly VerticalContainerProps Default = new(Box.Default, 2, VerticalAlign.Top);
}

public record struct VerticalContainer(in VerticalContainerProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class VerticalContainerFactory : IElementFactory
{
    private readonly ElementPool<VerticalContainer> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new VerticalContainerRenderer();

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

        return Fragment.From(ref element, _defaultElementRenderer);
    }
}

public class VerticalContainerRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<VerticalContainer>();
        var offsetInPixels = renderContext.Position
                             + Measure.ConvertEmToPixels(new Vector2D<float>(e.Props.Box.Padding.Left, e.Props.Box.Padding.Top));

        var gapSizeInPixels = Measure.ConvertEmToPixels(e.Props.GapSize);
        var contentHeightInPixels = 0f;
        var child0DimensionsInPixels = Vector2D<float>.Zero;
        var child1DimensionsInPixels = Vector2D<float>.Zero;
        var childCount = 0;

        if (e.Child0 is { PropertyAccessor: IBoxAccessor accessor0 }) {
            var box = accessor0.GetBox(e.Child0.Value);
            child0DimensionsInPixels = Measure.CalculateBoxDimensionsInPixels(box);
            contentHeightInPixels += child0DimensionsInPixels.Y;
            childCount++;
        }

        if (e.Child1 is { PropertyAccessor: IBoxAccessor accessor1 }) {
            var box = accessor1.GetBox(e.Child1.Value);
            child1DimensionsInPixels = Measure.CalculateBoxDimensionsInPixels(box);
            contentHeightInPixels += child1DimensionsInPixels.Y;
            childCount++;
        }

        contentHeightInPixels += gapSizeInPixels * MathF.Max(0, childCount - 1);

        switch (e.Props.VerticalAlignment) {
            case VerticalAlign.Center:
                var containerHeightInPixels = Measure.CalculateBoxHeightWithPadding(renderContext.ParentBoxInPixels) - Measure.CalculateBoxHeightWithPadding(renderContext.BoxInPixels);
                offsetInPixels += new Vector2D<float>(0, (containerHeightInPixels / 2f) - (contentHeightInPixels / 2f));
                break;

            case VerticalAlign.Bottom:
                var containerHeightInPixels2 = Measure.CalculateBoxHeightWithPadding(renderContext.ParentBoxInPixels) - Measure.CalculateBoxHeightWithPadding(renderContext.BoxInPixels);
                offsetInPixels += new Vector2D<float>(0, containerHeightInPixels2 - contentHeightInPixels);
                break;
        }


        if (e.Child0 is { PropertyAccessor: IBoxAccessor accessor2 }) {
            var box = accessor2.GetBox(e.Child0.Value);

            e.Child0.Value.ElementRenderer.Render(e.Child0.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = box,
                Font = renderContext.Font
            }, renderList);

            offsetInPixels += new Vector2D<float>(0, child0DimensionsInPixels.Y + gapSizeInPixels);
        }

        if (e.Child1 is { PropertyAccessor: IBoxAccessor accessor3 }) {
            var box = accessor3.GetBox(e.Child1.Value);

            e.Child1.Value.ElementRenderer.Render(e.Child1.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = box,
                Font = renderContext.Font
            }, renderList);

            offsetInPixels += new Vector2D<float>(0, child1DimensionsInPixels.Y + gapSizeInPixels);
        }
    }
}

public static class VerticalContainerExtensions
{
    public static Fragment VerticalContainer(this Context context, in VerticalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        return context.GetFactory<VerticalContainerFactory, VerticalContainer>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
