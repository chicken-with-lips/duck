using Silk.NET.Maths;

namespace Duck.Ui.Elements;

public readonly record struct HorizontalContainerProps(in Box Box, in float GapSize, in HorizontalAlign HorizontalAlignment)
{
    public static readonly HorizontalContainerProps Default = new(Box.Default, 2, HorizontalAlign.Left);
}

public record struct HorizontalContainer(in HorizontalContainerProps Props, in Fragment? Child0, in Fragment? Child1, in Fragment? Child2, in Fragment? Child3, in Fragment? Child4, in Fragment? Child5);

public class HorizontalContainerFactory : IElementFactory
{
    private readonly ElementPool<HorizontalContainer> _pool = new();
    private readonly IElementRenderer _defaultElementRenderer = new HorizontalContainerRenderer();

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

        return Fragment.From(ref element, _defaultElementRenderer);
    }
}

public class HorizontalContainerRenderer : IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList)
    {
        var e = fragment.GetElementAs<HorizontalContainer>();
        var offsetInPixels = renderContext.Position
                             + Measure.ConvertEmToPixels(new Vector2D<float>(e.Props.Box.Padding.Left, e.Props.Box.Padding.Top));

        var gapSizeInPixels = Measure.ConvertEmToPixels(e.Props.GapSize);
        var contentWidthInPixels = 0f;
        var child0DimensionsInPixels = Vector2D<float>.Zero;
        var child1DimensionsInPixels = Vector2D<float>.Zero;
        var childCount = 0;

        if (e.Child0 is { PropertyAccessor: IBoxAccessor accessor0 }) {
            var box = accessor0.GetBox(e.Child0.Value);
            child0DimensionsInPixels = Measure.CalculateBoxDimensionsInPixels(box);
            contentWidthInPixels += child0DimensionsInPixels.X;
            childCount++;
        }

        if (e.Child1 is { PropertyAccessor: IBoxAccessor accessor1 }) {
            var box = accessor1.GetBox(e.Child1.Value);
            child1DimensionsInPixels = Measure.CalculateBoxDimensionsInPixels(box);
            contentWidthInPixels += child1DimensionsInPixels.X;
            childCount++;
        }

        contentWidthInPixels += gapSizeInPixels * MathF.Max(0, childCount - 1);

        switch (e.Props.HorizontalAlignment) {
            case HorizontalAlign.Center:
                var containerWidthInPixels = Measure.CalculateBoxWidthWithPadding(renderContext.ParentBoxInPixels) - Measure.CalculateBoxWidthWithPadding(renderContext.BoxInPixels);
                offsetInPixels += new Vector2D<float>((containerWidthInPixels / 2f) - (contentWidthInPixels / 2f), 0);
                break;

            case HorizontalAlign.Right:
                var containerWidthInPixels2 = Measure.CalculateBoxWidthWithPadding(renderContext.ParentBoxInPixels) - Measure.CalculateBoxWidthWithPadding(renderContext.BoxInPixels);
                offsetInPixels += new Vector2D<float>(containerWidthInPixels2 - contentWidthInPixels, 0);
                break;
        }


        if (e.Child0 is { PropertyAccessor: IBoxAccessor accessor2 }) {
            var box = accessor2.GetBox(e.Child0.Value);

            e.Child0.Value.ElementRenderer.Render(e.Child0.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = box,
                Font = renderContext.Font
            }, renderList);

            offsetInPixels += new Vector2D<float>(child0DimensionsInPixels.X + gapSizeInPixels, 0);
        }

        if (e.Child1 is { PropertyAccessor: IBoxAccessor accessor3 }) {
            var box = accessor3.GetBox(e.Child1.Value);

            e.Child1.Value.ElementRenderer.Render(e.Child1.Value, ElementRenderContext.Default with {
                Position = offsetInPixels,
                Box = box,
                Font = renderContext.Font
            }, renderList);

            offsetInPixels += new Vector2D<float>(child1DimensionsInPixels.X + gapSizeInPixels, 0);
        }
    }
}

public static class HorizontalContainerExtensions
{
    public static Fragment HorizontalContainer(this Context context, in HorizontalContainerProps props, in Fragment? child0 = null, in Fragment? child1 = null, in Fragment? child2 = null, in Fragment? child3 = null, in Fragment? child4 = null, in Fragment? child5 = null)
    {
        return context.GetFactory<HorizontalContainerFactory, HorizontalContainer>().Create(props, child0, child1, child2, child3, child4, child5);
    }
}
