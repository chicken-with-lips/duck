using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class Writer
{
    public Writer()
    {
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

    public void Write(in uint value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in short value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in ushort value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in long value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in ulong value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in float value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in double value)
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

    public void Write(in int[] value)
    {
        ThrowIfSealed();

        _writer.Write(value.Length);

        for (var i = 0; i < value.Length; i++) _writer.Write(value[i]);
    }

    public void WriteGeneric<T>(in T value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        switch (Type.GetTypeCode(typeof(T))) {
            case TypeCode.Single:
                Write(Convert.ToSingle(value));

                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void Write(in Guid value)
    {
        ThrowIfSealed();

        Write(value.ToByteArray());
    }

    public void Write(in Enum value)
    {
        ThrowIfSealed();

        _writer.Write(value.ToString());
    }

    public void Write<T>(in IList<T> value, ISerializationContext context)
    {
        ThrowIfSealed();

        Write(value.Count);

        foreach (var e in value) {
            var childWriter = new GraphWriter(context);
            Serializer.Serialize(e, childWriter);
            Write(childWriter.Close());
        }
    }

    public void Write(in SerializedContainer container)
    {
        Write(container.Index.Count);
        Write(container.Data.Length);

        foreach (var entry in container.Index) {
            Write(entry.Name);
            Write((byte)entry.Type);
            Write(entry.OffsetStart);
            Write(entry.OffsetEnd);
            Write(entry.ExplicitType ?? "");
        }

        Write(container.Data.ToArray());
    }

    public ReadOnlyMemory<byte> Close()
    {
        ThrowIfSealed();

        IsSealed = true;

        return _stream.ToArray();
    }

    public void ThrowIfSealed()
    {
        if (IsSealed) {
            throw new SerializationException("Serializer is sealed");
        }
    }

    #region Properties

    public bool IsSealed { get; private set; }
    public long Position => _stream.Position;

    #endregion

    #region Members

    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;

    #endregion
}