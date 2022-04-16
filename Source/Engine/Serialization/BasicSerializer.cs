using System.Collections.ObjectModel;
using Duck.Serialization.Exception;
using Silk.NET.Maths;

namespace Duck.Serialization;

public class BasicSerializer : IBasicSerializer
{
    #region Properties

    public bool IsSealed { get; private set; }
    public long Position => _stream.Position;

    #endregion

    #region Members

    private readonly ISerializationContext _context;
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;

    #endregion

    public BasicSerializer(ISerializationContext context)
    {
        _context = context;
        _stream = new MemoryStream();
        _writer = new BinaryWriter(_stream);
    }

    public void Write(in string value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in int value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in long value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in float value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in bool value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in byte value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in byte[] value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in Vector3D<float> value)
    {
        ThrowIfSealed();

        _writer.Write(value.X);
        _writer.Write(value.Y);
        _writer.Write(value.Z);
    }

    public void Write(in Vector2D<float> value)
    {
        ThrowIfSealed();

        _writer.Write(value.X);
        _writer.Write(value.Y);
    }

    public void Write(in Box3D<float> value)
    {
        ThrowIfSealed();

        Write(value.Min);
        Write(value.Max);
    }

    public void Write(in Quaternion<float> value)
    {
        ThrowIfSealed();

        _writer.Write(value.X);
        _writer.Write(value.Y);
        _writer.Write(value.Z);
        _writer.Write(value.W);
    }

    public SerializedContainer Close()
    {
        IsSealed = true;

        return new SerializedContainer() {
            Index = new ReadOnlyCollection<IndexEntry>(new List<IndexEntry>()),
            Data = _stream.ToArray(),
        };
    }

    private void ThrowIfSealed()
    {
        if (IsSealed) {
            throw new SerializationException("Serializer is sealed");
        }
    }
}
