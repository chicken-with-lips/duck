using System.Collections.ObjectModel;
using Silk.NET.Maths;

namespace Duck.Serialization;

public class Deserializer : IDeserializer
{
    #region Properties

    public ReadOnlyCollection<IndexEntry> Index { get; }

    public IDeserializer Root {
        get {
            IDeserializer current = this;

            while (current.Parent != null) {
                current = current.Parent;
            }

            return current;
        }
    }

    public IDeserializer? Parent { get; }

    #endregion

    #region Members

    private readonly MemoryStream _stream;
    private readonly BinaryReader _reader;
    private readonly ISerializationContext _context;

    #endregion

    #region Constructor

    public Deserializer(in ReadOnlyMemory<byte> data, ReadOnlyCollection<IndexEntry> index, ISerializationContext context, IDeserializer? parent = null)
    {
        Index = index;
        Parent = parent;

        _context = context;

        _stream = new MemoryStream(data.ToArray(), false);
        _reader = new BinaryReader(_stream);
    }

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
        return _reader.ReadSingle();
    }

    public float ReadFloat(long offset)
    {
        _stream.Position = offset;

        return ReadFloat();
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

    public Vector3D<float> ReadVector3D()
    {
        return new Vector3D<float>(
            ReadFloat(),
            ReadFloat(),
            ReadFloat()
        );
    }

    public Vector3D<float> ReadVector3D(long offset)
    {
        _stream.Position = offset;

        return ReadVector3D();
    }

    public Vector2D<float> ReadVector2D()
    {
        return new Vector2D<float>(
            ReadFloat(),
            ReadFloat()
        );
    }

    public Vector2D<float> ReadVector2D(long offset)
    {
        _stream.Position = offset;

        return ReadVector2D();
    }

    public Box3D<float> ReadBox3D()
    {
        return new Box3D<float>(
            ReadVector3D(),
            ReadVector3D()
        );
    }

    public Box3D<float> ReadBox3D(long offset)
    {
        _stream.Position = offset;

        return ReadBox3D();
    }

    public Quaternion<float> ReadQuaternion()
    {
        return new Quaternion<float>(
            ReadFloat(),
            ReadFloat(),
            ReadFloat(),
            ReadFloat()
        );
    }

    public Quaternion<float> ReadQuaternion(long offset)
    {
        _stream.Position = offset;

        return ReadQuaternion();
    }

    public T ReadObjectInternal<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator, long? offset, int? objectId) where T : ISerializable
    {
        if (offset != null) {
            _stream.Position = offset.Value;
        }
        
        var container = ReadSerializedContainer();
        var deserializer = new Deserializer(container.Data, container.Index, _context, this);
        var context = new DeserializationContext(objectId, _context);

        return objectInstanciator(deserializer, context);
    }

    public T ReadObject<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator) where T : ISerializable
    {
        return ReadObjectInternal(objectInstanciator, null, null);
    }

    public T ReadObject<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator, long offset) where T : ISerializable
    {
        return ReadObjectInternal(objectInstanciator, offset, null);
    }

    public T ReadObjectReference<T>(IDeserializer.ObjectReferenceInstanciator<T> objectInstanciator, IndexEntry lookup) where T : ISerializable
    {
        int hash = lookup.GetHashCode();

        if (_context.HasObject(hash)) {
            return (T)_context.GetObject(hash);
        }

        if (!Root.Equals(this)) {
            var resolvedEntry = Root.Index[(int)lookup.OffsetStart];

            return Root.ReadObjectReference(objectInstanciator, resolvedEntry);
        }

        return ReadObjectInternal((d, c) => objectInstanciator(d, c, lookup), lookup.OffsetStart, lookup.GetHashCode());
    }

    public T1 ReadObjectList<T1, T2>(IDeserializer.NestedObjectInstanciator<T2> objectInstanciator, string containerType)
        where T1 : class
        where T2 : ISerializable
    {
        var serializedContainer = ReadSerializedContainer();
        var objectList = new List<T2>();
        var deserializer = new Deserializer(serializedContainer.Data, serializedContainer.Index, _context, this);

        foreach (var entry in serializedContainer.Index) {
            T2 obj;

            if (entry.Type == DataType.ReferenceObject) {
                obj = deserializer.ReadObjectReference((d, c, e) => objectInstanciator(d, c, e), entry);
            } else {
                obj = deserializer.ReadObject((d, c) => objectInstanciator(d, c, entry), entry.OffsetStart);
            }

            objectList.Add(
                obj
            );
        }

        if (containerType == "System.Collections.Generic.List") {
            return objectList as T1;
        }

        throw new NotImplementedException();
    }

    public T1 ReadObjectList<T1, T2>(IDeserializer.NestedObjectInstanciator<T2> objectInstanciator, string containerType, long offset)
        where T1 : class
        where T2 : ISerializable
    {
        _stream.Position = offset;

        return ReadObjectList<T1, T2>(objectInstanciator, containerType);
    }

    private SerializedContainer ReadSerializedContainer()
    {
        var indexCount = ReadInt32();
        var dataLength = ReadInt32();

        var index = new IndexEntry[indexCount];

        for (var i = 0; i < indexCount; i++) {
            index[i] = new IndexEntry() {
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

        return new SerializedContainer() {
            Index = new ReadOnlyCollection<IndexEntry>(index),
            Data = ReadBytes(dataLength)
        };
    }

    #endregion
}
