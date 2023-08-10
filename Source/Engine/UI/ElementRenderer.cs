﻿using Duck.Content;
using Duck.Input;
using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public interface IElementRenderer
{
    public void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList);
}

public readonly record struct ElementRenderContext(in Vector2D<float> Position, in Box ParentBox, in Box ParentBoxInPixels, in Box ContainerBox, in Box ContainerBoxInPixels, in AssetReference<Font>? Font, in IInputModule Input)
{
    public static readonly ElementRenderContext Default = new();
}

public abstract class ElementRendererBase : IElementRenderer
{
    public delegate Vector2D<float> CalculateOffsetCallback(in Box box, in Vector2D<float> position, in Vector2D<float> gapSize);

    public abstract void Render(in Fragment fragment, in ElementRenderContext renderContext, RenderList renderList);

    public void RenderChildren(in Vector2D<float> position, in Vector2D<float> gapSize, in ElementRenderContext renderContext, RenderList renderList, in Fragment? fragment0, in Fragment? fragment1, in Fragment? fragment2, in Fragment? fragment3, in Fragment? fragment4, in Fragment? fragment5, CalculateOffsetCallback calculateOffset)
    {
        var offset = position;

        if (fragment0 is { PropertyAccessor: IBoxAccessor accessor0 }) {
            var box = accessor0.GetBox(fragment0.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment0.Value.ElementRenderer.Render(fragment0.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }

        if (fragment1 is { PropertyAccessor: IBoxAccessor accessor1 }) {
            var box = accessor1.GetBox(fragment1.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment1.Value.ElementRenderer.Render(fragment1.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }

        if (fragment2 is { PropertyAccessor: IBoxAccessor accessor2 }) {
            var box = accessor2.GetBox(fragment2.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment2.Value.ElementRenderer.Render(fragment2.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }

        if (fragment3 is { PropertyAccessor: IBoxAccessor accessor3 }) {
            var box = accessor3.GetBox(fragment3.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment3.Value.ElementRenderer.Render(fragment3.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }

        if (fragment4 is { PropertyAccessor: IBoxAccessor accessor4 }) {
            var box = accessor4.GetBox(fragment4.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment4.Value.ElementRenderer.Render(fragment4.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }

        if (fragment5 is { PropertyAccessor: IBoxAccessor accessor5 }) {
            var box = accessor5.GetBox(fragment5.Value);
            var containerBox = Box.Default with {
                ContentHeight = Measure.BoxHeight(renderContext.ContainerBox) - Measure.BoxHeight(box),
                ContentWidth = Measure.BoxWidth(renderContext.ContainerBox) - Measure.BoxWidth(box),
            };

            fragment5.Value.ElementRenderer.Render(fragment5.Value, renderContext with {
                ContainerBox = containerBox,
                ContainerBoxInPixels = Measure.EmToPixels(containerBox),
                Font = renderContext.Font,
                ParentBox = box,
                ParentBoxInPixels = Measure.EmToPixels(box),
                Position = offset,
            }, renderList);

            offset = calculateOffset(box, offset, gapSize);
        }
    }

    public void RenderChildrenHorizontal(in Vector2D<float> position, in Vector2D<float> gapSize, in ElementRenderContext renderContext, RenderList renderList, in Fragment? fragment0, in Fragment? fragment1, in Fragment? fragment2, in Fragment? fragment3, in Fragment? fragment4, in Fragment? fragment5)
    {
        RenderChildren(
            position,
            gapSize,
            renderContext,
            renderList,
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5,
            (in Box box, in Vector2D<float> position, in Vector2D<float> gapSize) => position + new Vector2D<float>(Measure.BoxDimensions(box).X + gapSize.X, 0)
        );
    }

    public void RenderChildrenHorizontal(in Fragment parent, in Vector2D<float> gapSize, in ElementRenderContext renderContext, RenderList renderList, in Fragment? fragment0, in Fragment? fragment1, in Fragment? fragment2, in Fragment? fragment3, in Fragment? fragment4, in Fragment? fragment5)
    {
        var box = Box.Default;

        if (parent is { PropertyAccessor: IBoxAccessor accessor }) {
            box = accessor.GetBox(parent);
        }

        RenderChildren(
            Measure.ContentPosition(renderContext, box),
            gapSize,
            renderContext,
            renderList,
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5,
            (in Box box, in Vector2D<float> position, in Vector2D<float> gapSize) => position + new Vector2D<float>(Measure.BoxDimensions(box).X + gapSize.X, 0)
        );
    }

    public void RenderChildrenVertical(in Vector2D<float> position, in Vector2D<float> gapSize, in ElementRenderContext renderContext, RenderList renderList, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        RenderChildren(
            position,
            gapSize,
            renderContext,
            renderList,
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5,
            (in Box box, in Vector2D<float> position, in Vector2D<float> gapSize) => position + new Vector2D<float>(0, Measure.BoxDimensions(box).Y + gapSize.Y)
        );
    }

    public void RenderChildrenVertical(in Fragment parent, in Vector2D<float> gapSize, in ElementRenderContext renderContext, RenderList renderList, in Fragment? fragment0, in Fragment? fragment1 = null, in Fragment? fragment2 = null, in Fragment? fragment3 = null, in Fragment? fragment4 = null, in Fragment? fragment5 = null)
    {
        RenderChildren(
            Measure.ContentPosition(renderContext, parent),
            gapSize,
            renderContext,
            renderList,
            fragment0,
            fragment1,
            fragment2,
            fragment3,
            fragment4,
            fragment5,
            (in Box box, in Vector2D<float> position, in Vector2D<float> gapSize) => position + new Vector2D<float>(0, Measure.BoxDimensions(box).Y + gapSize.Y)
        );
    }
}
