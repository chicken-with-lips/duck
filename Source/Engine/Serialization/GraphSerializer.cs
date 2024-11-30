using ADyn;
using ADyn.Shapes;
using Arch.Core;
using Duck.Content;
using Duck.Serialization.Exception;
using Silk.NET.Maths;

namespace Duck.Serialization;

public class GraphSerializer : IGraphSerializer
{
    #region Properties

    public bool IsSealed { get; private set; }

    public IGraphSerializer Root {
        get {
            IGraphSerializer current = this;

            while (current.Parent != null) {
                current = current.Parent;
            }

            return current;
        }
    }

    public IGraphSerializer? Parent { get; }

    #endregion

    #region Members

    private readonly List<IndexEntry> _index;
    private readonly Dictionary<int, DeferredSerialization> _deferred;
    private readonly Dictionary<int, int> _deferredLookup;
    private readonly DefaultBasicSerializer _basicSerializer;
    private readonly ISerializationContext _context;
    #endregion

    public GraphSerializer(ISerializationContext context, IGraphSerializer? parent = null)
    {
        Parent = parent;

        _context = context;
        _basicSerializer = new DefaultBasicSerializer();
        _index = new();
        _deferred = new();
        _deferredLookup = new();
    }

    public void Write(string name, in string value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.String, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in int value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Int32, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in float value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Float, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in bool value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Boolean, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Vector4D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Vector4D, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Vector3D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Vector3D, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Vector2D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Vector2D, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Box3D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Box3D, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Quaternion<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Matrix3X3<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Matrix3X3, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in Matrix4X4<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Matrix4X4, offsetStart, _basicSerializer.Position);
    }

    public void Write<T>(string name, in AssetReference<T> value) where T : class, IAsset
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _basicSerializer.Position);
    }

    public void Write<TShapeType>(string name, in RigidBodyDefinition<TShapeType> value) where TShapeType : unmanaged, IShape
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.RigidBodyDefinition, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, EntityReference value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;

        _basicSerializer.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _basicSerializer.Position);
    }

    public void Write(string name, in object value)
    {
        ThrowIfSealed();

        var offsetStart = _basicSerializer.Position;
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
            offsetEnd = _basicSerializer.Position;
        }

        PushIndex(name, dataType, offsetStart, offsetEnd, explicitType);
    }

   /* public void Write(string name, in ISerializable[] value, string containerType)
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
        int hash = value.GetHashCode();

        if (_deferredLookup.TryGetValue(hash, out var outValue)) {
            return outValue;
        }

        int index = _index.Count;

        _index.Add(new IndexEntry() {
            Name = hash.ToString(),
            Type = DataType.ReferenceObject,
        });

        _deferred.Add(hash, new DeferredSerialization() {
            Index = index,
            Object = value,
        });

        _deferredLookup.Add(hash, index);

        return index;
    }

    private void PushIndex(string name, DataType type, long offsetStart, long offsetEnd, string? explicitType = null)
    {
        _index.Add(
            new IndexEntry() {
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
        var serializer = new GraphSerializer(_context, this);
        
        Serializer.Serialize(obj, serializer, _context);

        var container = serializer.Close();

        WriteSerializedContainer(container);
    }

    private void WriteSerializedContainer(SerializedContainer container)
    {
        _basicSerializer.Write(container.Index.Count);
        _basicSerializer.Write(container.Data.Length);

        foreach (var entry in container.Index) {
            _basicSerializer.Write(entry.Name);
            _basicSerializer.Write((byte)entry.Type);
            _basicSerializer.Write(entry.OffsetStart);
            _basicSerializer.Write(entry.OffsetEnd);
            _basicSerializer.Write(entry.ExplicitType ?? "");
        }

        _basicSerializer.Write(container.Data.ToArray());
    }

    private void FlushDeferredSerializationList()
    {
        while (_deferred.Count > 0) {
            var toSerialize = _deferred.ToArray();
            _deferred.Clear();

            foreach (var kvp in toSerialize) {
                var offsetStart = _basicSerializer.Position;

                WriteSerializableObject(kvp.Value.Object);

                _index[kvp.Value.Index] = new IndexEntry() {
                    Name = kvp.Value.Object.GetHashCode().ToString(),
                    Type = DataType.ReferenceObject,
                    OffsetStart = offsetStart,
                    OffsetEnd = _basicSerializer.Position,
                    ExplicitType = kvp.Value.Object.GetType().FullName
                };
            }
        }
    }

    public SerializedContainer Close()
    {
        FlushDeferredSerializationList();

        IsSealed = true;

        var container = _basicSerializer.Close();

        return container with {
            Index = _index.AsReadOnly(),
        };
    }

    private void ThrowIfSealed()
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
