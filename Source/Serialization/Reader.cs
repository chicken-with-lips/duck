using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class Reader
{
    #region Constructor

    public Reader(in ReadOnlyMemory<byte> data, ISerializationContext context)
    {
        Context = context;

        _stream = new MemoryStream(data.ToArray(), false);
        _reader = new BinaryReader(_stream);
    }

    #endregion

    #region Properties

    public ISerializationContext Context { get; }

    public long Position {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    #endregion

    #region Members

    private readonly MemoryStream _stream;
    private readonly BinaryReader _reader;

    #endregion

    #region Methods

    public string ReadString()
    {
        return _reader.ReadString();
    }

    public string ReadString(long offset)
    {
        _stream.Position = offset;

        return ReadString();
    }

    public int ReadInt32()
    {
        return _reader.ReadInt32();
    }

    public int ReadInt32(long offset)
    {
        _stream.Position = offset;

        return ReadInt32();
    }

    public ushort ReadUInt16()
    {
        return _reader.ReadUInt16();
    }

    public ushort ReadUInt16(long offset)
    {
        _stream.Position = offset;

        return ReadUInt16();
    }

    public uint ReadUInt32()
    {
        return _reader.ReadUInt32();
    }

    public uint ReadUInt32(long offset)
    {
        _stream.Position = offset;

        return ReadUInt32();
    }

    public ulong ReadUInt64()
    {
        return _reader.ReadUInt64();
    }

    public ulong ReadUInt64(long offset)
    {
        _stream.Position = offset;

        return ReadUInt64();
    }

    public long ReadInt64()
    {
        return _reader.ReadInt64();
    }

    public long ReadInt64(long offset)
    {
        _stream.Position = offset;

        return ReadInt64();
    }

    public float ReadFloat()
    {
        return ReadSingle();
    }

    public float ReadFloat(long offset)
    {
        return ReadSingle(offset);
    }

    public float ReadSingle()
    {
        return _reader.ReadSingle();
    }

    public float ReadSingle(long offset)
    {
        _stream.Position = offset;

        return ReadSingle();
    }

    public double ReadDouble()
    {
        return _reader.ReadDouble();
    }

    public double ReadDouble(long offset)
    {
        _stream.Position = offset;

        return ReadDouble();
    }

    public double? ReadNullOrDouble()
    {
        var hasValue = ReadBoolean();

        if (!hasValue) {
            return null;
        }

        return _reader.ReadDouble();
    }

    public double? ReadNullOrDouble(long offset)
    {
        _stream.Position = offset;

        return ReadNullOrDouble();
    }

    public bool ReadBoolean()
    {
        return _reader.ReadBoolean();
    }

    public bool ReadBoolean(long offset)
    {
        _stream.Position = offset;

        return ReadBoolean();
    }

    public byte ReadByte()
    {
        return _reader.ReadByte();
    }

    public byte ReadByte(long offset)
    {
        _stream.Position = offset;

        return ReadByte();
    }

    public ReadOnlyMemory<byte> ReadBytes(long count)
    {
        var data = new byte[count];
        var read = _stream.Read(data);

        if (read < count) {
            throw new InvalidDataException("Could not read expected count");
        }

        return data;
    }

    public ReadOnlyMemory<byte> ReadBytes(long count, long offset)
    {
        _stream.Position = offset;

        return ReadBytes(count);
    }

    public Guid ReadGuid()
    {
        return new Guid(
            ReadBytes(16).Span
        );
    }

    public Guid ReadGuid(long offset)
    {
        _stream.Position = offset;

        return ReadGuid();
    }

    public T ReadGeneric<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        switch (Type.GetTypeCode(typeof(T))) {
            case TypeCode.Single: {
                var value = ReadSingle();
                return Unsafe.As<float, T>(ref value);
            }
            case TypeCode.Double: {
                var value = ReadDouble();
                return Unsafe.As<double, T>(ref value);
            }
            default:
                throw new NotImplementedException();
        }
    }

    private T ReadEnum<T>() where T : struct
    {
        var str = ReadString();

        return Enum.Parse<T>(str);
    }

    public object ReadObject(string typeName)
    {
        var container = ReadSerializedContainer();
        var deserializer = new GraphReader(container.Data, container.Index, Context);

        return Serializer.Deserialize(typeName, deserializer);
    }

    public object ReadObject(string typeName, long offset)
    {
        _stream.Position = offset;

        return ReadObject(typeName);
    }

    public T ReadObject<T>()
    {
        return (T)ReadObject(GetTypeName<T>());
    }

    public T ReadObject<T>(long offset)
    {
        return (T)ReadObject(GetTypeName<T>(), offset);
    }

    private static string GetTypeName<T>()
    {
        return typeof(T).FullName
               ?? throw new SerializationException($"Type '{typeof(T)}' has no full name and cannot be deserialized");
    }

    public void ReadObjectList<TContainerType, TElementType>(ref TContainerType dest)
        where TContainerType : class, IList
    {
        var objectCount = ReadInt32();

        for (var i = 0; i < objectCount; i++) dest[i] = ReadObject<TElementType>();
    }

    public SerializedContainer ReadSerializedContainer()
    {
        var indexCount = ReadInt32();
        var dataLength = ReadInt32();

        var index = new IndexEntry[indexCount];

        for (var i = 0; i < indexCount; i++) {
            index[i] = new IndexEntry {
                Name = ReadString(),
                Type = (DataType)ReadByte(),
                OffsetStart = ReadInt64(),
                OffsetEnd = ReadInt64(),
                ExplicitType = ReadString(),
            };

            if (string.IsNullOrEmpty(index[i].ExplicitType)) {
                index[i].ExplicitType = null;
            }
        }

        return new SerializedContainer {
            Index = new ReadOnlyCollection<IndexEntry>(index),
            Data = ReadBytes(dataLength),
        };
    }

    public int[] ReadInt32Array()
    {
        var length = ReadInt32();
        var array = new int[length];

        for (var i = 0; i < length; i++) array[i] = ReadInt32();

        return array;
    }

    #endregion
}