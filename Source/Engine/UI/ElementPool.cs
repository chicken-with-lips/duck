namespace Duck.Ui;

public class ElementPool<T>
    where T : struct
{
    private const int InitialCapacity = 100;

    private T[] _elements;
    private int _index;

    public ElementPool()
    {
        _elements = new T[InitialCapacity];
        _index = 0;
    }

    public void ResetIndex()
    {
        _index = 0;
    }

    public ref T Allocate()
    {
        _elements[_index] = default;

        return ref _elements[_index++];
    }

    public Span<T> ToSpan()
    {
        return _elements.AsSpan(0, _index);
    }
}
