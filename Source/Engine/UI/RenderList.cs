using System.Drawing;
using Duck.Content;
using Duck.Ui.Elements;
using Silk.NET.Maths;

namespace Duck.Ui;

public class RenderList
{
    #region Properties

    public Span<RenderPrimitive> Primitives => _primitives.AsSpan(0, _primitiveIndex);
    public int BoxCount => _boxCount;
    public int TextCharacterCount => _textCharacterCount;

    #endregion

    #region Members

    private readonly RenderPrimitive[] _primitives = new RenderPrimitive[1000];
    private int _primitiveIndex = 0;
    private int _boxCount = 0;
    private int _textCharacterCount = 0;

    #endregion

    public void Clear()
    {
        _primitiveIndex = 0;
        _boxCount = 0;
        _textCharacterCount = 0;
    }

    public void DrawBox(in Vector2D<float> position, in Vector2D<float> dimensions)
    {
        DrawBox(position, dimensions, Color.White);
    }

    public void DrawBox(in Vector2D<float> position, in Box dimensions)
    {
        DrawBox(position, Measure.BoxDimensions(dimensions), Color.White);
    }

    public void DrawBox(in Vector2D<float> position, in Box dimensions, Color color)
    {
        DrawBox(position, Measure.BoxDimensions(dimensions), color);
    }

    public void DrawBox(in Vector2D<float> position, in Vector2D<float> dimensions, Color color)
    {
        _primitives[_primitiveIndex++] = new RenderPrimitive {
            Type = RenderPrimitiveType.Box,
            Position = position,
            Dimensions = dimensions,
            Color = color,
        };

        _boxCount++;
    }

    public void DrawText(AssetReference<Font>? font, string content, Vector2D<float> position)
    {
        _primitives[_primitiveIndex++] = new RenderPrimitive {
            Type = RenderPrimitiveType.Text,
            Position = position,
            Text = content,
            Font = font,
            Color = Color.Blue,
        };

        _textCharacterCount += content.Length;
    }
}

public readonly record struct RenderPrimitive(RenderPrimitiveType Type, in Vector2D<float> Position, in Vector2D<float> Dimensions, in Color Color, in AssetReference<Font>? Font, in string? Text);

public enum RenderPrimitiveType
{
    Box,
    Text
}
