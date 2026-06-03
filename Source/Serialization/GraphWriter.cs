using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class GraphWriter
{
    #region Properties

    public bool IsSealed { get; private set; }

    public ISerializationContext Context { get; }

    public Writer Writer { get; }

    #endregion

    #region Members

    private readonly List<IndexEntry> _index;
    private readonly Dictionary<object, DeferredSerialization> _deferred;
    private readonly Dictionary<object, int> _deferredLookup;

    #endregion

    public GraphWriter(ISerializationContext context)
    {
        Context = context;
        Writer = new Writer();
        _index = new List<IndexEntry>();
        _deferred = new Dictionary<object, DeferredSerialization>(ReferenceEqualityComparer.Instance);
        _deferredLookup = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
    }

    public void Write(string name, in string value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value);
        PushIndex(name, DataType.String, offsetStart, Writer.Position);
    }

    public void Write(string name, in int value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value);
        PushIndex(name, DataType.Int32, offsetStart, Writer.Position);
    }

    public void Write(string name, in float value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value);
        PushIndex(name, DataType.Float, offsetStart, Writer.Position);
    }

    public void WriteNullOr(string name, in float? value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value.HasValue);

        if (value.HasValue) {
            Writer.Write(value.Value);
        }

        PushIndex(name, DataType.NullOrFloat, offsetStart, Writer.Position);
    }

    public void Write(string name, in double value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value);
        PushIndex(name, DataType.Double, offsetStart, Writer.Position);
    }

    public void WriteNullOr(string name, in double? value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value.HasValue);

        if (value.HasValue) {
            Writer.Write(value.Value);
        }

        PushIndex(name, DataType.NullOrDouble, offsetStart, Writer.Position);
    }

    public void Write(string name, in bool value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value);
        PushIndex(name, DataType.Boolean, offsetStart, Writer.Position);
    }

    /*public void Write<T>(string name, in AssetReference<T> value) where T : class, IAsset
    {
        ThrowIfSealed();

        var offsetStart = _primitiveSerializer.Position;

        _primitiveSerializer.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _primitiveSerializer.Position);
    }*/

    /*public void Write<TShapeType>(string name, in RigidBodyDefinition<TShapeType> value) where TShapeType : unmanaged, IShape
    {
        ThrowIfSealed();

        var offsetStart = _primitiveSerializer.Position;

        _primitiveSerializer.Write(value);
        PushIndex(name, DataType.RigidBodyDefinition, offsetStart, _primitiveSerializer.Position);
    }*/

    public void Write<T>(string name, in IList<T> value)
    {
        ThrowIfSealed();

        var offsetStart = Writer.Position;

        Writer.Write(value, Context);
        PushIndex(name, DataType.GenericList, offsetStart, Writer.Position);
    }

    /*public void WriteNullOr<T>(string name, in IList<ConstraintRow>? value)
    {
        ThrowIfSealed();

        var offsetStart = _primitiveSerializer.Position;
        var hasValue = null != value;

        _primitiveSerializer.Write(hasValue);

        if (hasValue) {
            // _primitiveSerializer.Write();

            foreach (var e in value) {
                // _primitiveSerializer.Write(e);
            }
        }

        PushIndex(name, DataType.NullOrList, offsetStart, _primitiveSerializer.Position);
    }*/

    // [GenerateListSerializer<ConstraintRow>]
    // public partial void Write(string name, in IList<ConstraintRow> value);
    /*{
        ThrowIfSealed();

        var offsetStart = _primitiveSerializer.Position;

        _primitiveSerializer.Write(value.Count);

        foreach (var e in value) {
            // _primitiveSerializer.Write(e);
        }

        PushIndex(name, DataType.GenericList, offsetStart, _primitiveSerializer.Position);
    }*/

    /*public void Write(string name, in object value)
    {
        ThrowIfSealed();

        if (value is IList listValue) {
            Write(name, listValue);
            return;
        }

        var offsetStart = _writer.Position;
        var offsetEnd = (long)0;
        var dataType = DataType.ValueObject;
        string? explicitType = null;
        bool shouldSerializeNow = true;

        // this is a reference type. add the object to the root serializer and store a reference here
        if (!value.GetType().IsValueType) {
            dataType = DataType.ReferenceObject;

            if (!Root.Equals(this)) {
                explicitType = null;
                offsetStart = Root.TrackReferenceAndDeferSerialization(value);
                shouldSerializeNow = false;
            }
        }

        if (shouldSerializeNow) {
            WriteSerializableObject(value);

            explicitType = value.GetType().FullName;
            offsetEnd = _writer.Position;
        }

        PushIndex(name, dataType, offsetStart, offsetEnd, explicitType);
    }*/

    /*public void Write(string name, in IList value)
    {
        ThrowIfSealed();

        var serializer = new GraphSerializer(_context, this);

        for (var i = 0; i < value.Length; i++) {
            serializer.Write(i.ToString(), value[i]);
        }

        var container = serializer.Close();
        var offsetStart = _serializer.Position;

        WriteSerializedContainer(container);
        PushIndex(name, DataType.ValueObject, offsetStart, _serializer.Position, containerType);
    }*/

    public int TrackReferenceAndDeferSerialization(in object value)
    {
        if (_deferredLookup.TryGetValue(value, out var outValue)) {
            return outValue;
        }

        var index = _index.Count;

        _index.Add(new IndexEntry {
                Name = RuntimeHelpers
                    .GetHashCode(value)
                    .ToString(),
                Type = DataType.ReferenceObject,
            }
        );

        _deferred.Add(value,
            new DeferredSerialization {
                Index = index,
                Object = value,
            }
        );

        _deferredLookup.Add(value, index);

        return index;
    }

    public void PushIndex(string name, DataType type, long offsetStart, long offsetEnd, string? explicitType = null)
    {
        _index.Add(new IndexEntry {
                Name = name,
                Type = type,
                OffsetStart = offsetStart,
                OffsetEnd = offsetEnd,
                ExplicitType = explicitType,
            }
        );
    }

    private void WriteSerializableObject(object obj)
    {
        var writer = new GraphWriter(Context);

        Serializer.Serialize(obj, writer);

        var container = writer.Close();

        WriteSerializedContainer(container);
    }

    private void WriteSerializedContainer(SerializedContainer container)
    {
        Writer.Write(container.Index.Count);
        Writer.Write(container.Data.Length);

        foreach (var entry in container.Index) {
            Writer.Write(entry.Name);
            Writer.Write((byte)entry.Type);
            Writer.Write(entry.OffsetStart);
            Writer.Write(entry.OffsetEnd);
            Writer.Write(entry.ExplicitType ?? "");
        }

        Writer.Write(container.Data.ToArray());
    }

    private void FlushDeferredSerializationList()
    {
        while (_deferred.Count > 0) {
            var toSerialize = _deferred.ToArray();
            _deferred.Clear();

            foreach (var kvp in toSerialize) {
                var offsetStart = Writer.Position;

                WriteSerializableObject(kvp.Value.Object);

                _index[kvp.Value.Index] = new IndexEntry {
                    Name = RuntimeHelpers
                        .GetHashCode(kvp.Value.Object)
                        .ToString(),
                    Type = DataType.ReferenceObject,
                    OffsetStart = offsetStart,
                    OffsetEnd = Writer.Position,
                    ExplicitType = kvp.Value.Object.GetType()
                        .FullName,
                };
            }
        }
    }

    public SerializedContainer Close()
    {
        FlushDeferredSerializationList();

        IsSealed = true;

        var data = Writer.Close();

        return new SerializedContainer {
            Index = _index.AsReadOnly(),
            Data = data,
        };
    }

    public void ThrowIfSealed()
    {
        if (IsSealed) {
            throw new SerializationException("Serializer is sealed");
        }
    }

    private struct DeferredSerialization
    {
        public int Index;
        public object Object;
    }
}

public struct SerializedContainer
{
    public ReadOnlyCollection<IndexEntry> Index;
    public ReadOnlyMemory<byte> Data;
}

public struct IndexEntry
{
    public string Name;
    public DataType Type;
    public long OffsetStart;
    public long OffsetEnd;
    public string? ExplicitType;
}

public enum DataType : byte
{
    Boolean = 1,
    Double = 2,
    Float = 3,
    Int32 = 4,
    String = 5,
    Vector2D = 6,
    Vector3D = 7,
    Vector4D = 8,
    Box3D = 9,
    Quaternion = 10,
    Matrix3X3 = 11,
    Matrix4X4 = 12,
    ValueObject = 14,
    ReferenceObject = 15,
    GenericList = 16,
    List = 17,
    NullOrList = 18,
    NullOrFloat = 19,
    NullOrDouble = 20,
    Entity = 21,
}
