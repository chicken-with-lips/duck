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
    private readonly StandardSerializer _standard;
    private readonly ISerializationContext _context;

    #endregion

    public GraphSerializer(ISerializationContext context, IGraphSerializer? parent = null)
    {
        Parent = parent;

        _context = context;
        _standard = new StandardSerializer(context);
        _index = new();
        _deferred = new();
        _deferredLookup = new();
    }

    public void Write(string name, in string value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.String, offsetStart, _standard.Position);
    }

    public void Write(string name, in int value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Int32, offsetStart, _standard.Position);
    }

    public void Write(string name, in float value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Float, offsetStart, _standard.Position);
    }

    public void Write(string name, in bool value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Boolean, offsetStart, _standard.Position);
    }

    public void Write(string name, in Vector3D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Vector3D, offsetStart, _standard.Position);
    }

    public void Write(string name, in Vector2D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Vector2D, offsetStart, _standard.Position);
    }

    public void Write(string name, in Box3D<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Box3D, offsetStart, _standard.Position);
    }

    public void Write(string name, in Quaternion<float> value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _standard.Position);
    }

    public void Write<T>(string name, in AssetReference<T> value) where T : class, IAsset
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _standard.Position);
    }

    public void Write(string name, EntityReference value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;

        _standard.Write(value);
        PushIndex(name, DataType.Quaternion, offsetStart, _standard.Position);
    }

    public void Write(string name, in ISerializable value)
    {
        ThrowIfSealed();

        var offsetStart = _standard.Position;
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
            offsetEnd = _standard.Position;
        }

        PushIndex(name, dataType, offsetStart, offsetEnd, explicitType);
    }

    public void Write(string name, in ISerializable[] value, string containerType)
    {
        ThrowIfSealed();

        var serializer = new GraphSerializer(_context, this);

        for (var i = 0; i < value.Length; i++) {
            serializer.Write(i.ToString(), value[i]);
        }

        var container = serializer.Close();
        var offsetStart = _standard.Position;

        WriteSerializedContainer(container);
        PushIndex(name, DataType.ValueObject, offsetStart, _standard.Position, containerType);
    }

    public int TrackReferenceAndDeferSerialization(in ISerializable value)
    {
        int hash = value.GetHashCode();

        if (_deferredLookup.ContainsKey(hash)) {
            return _deferredLookup[hash];
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

    private void WriteSerializableObject(ISerializable obj)
    {
        var serializer = new GraphSerializer(_context, this);
        obj.Serialize(serializer, _context);
        var container = serializer.Close();

        WriteSerializedContainer(container);
    }

    private void WriteSerializedContainer(SerializedContainer container)
    {
        _standard.Write(container.Index.Count);
        _standard.Write(container.Data.Length);

        foreach (var entry in container.Index) {
            _standard.Write(entry.Name);
            _standard.Write((byte)entry.Type);
            _standard.Write(entry.OffsetStart);
            _standard.Write(entry.OffsetEnd);
            _standard.Write(entry.ExplicitType ?? "");
        }

        _standard.Write(container.Data.ToArray());
    }

    private void FlushDeferredSerializationList()
    {
        while (_deferred.Count > 0) {
            var toSerialize = _deferred.ToArray();
            _deferred.Clear();

            foreach (var kvp in toSerialize) {
                var offsetStart = _standard.Position;

                WriteSerializableObject(kvp.Value.Object);

                _index[kvp.Value.Index] = new IndexEntry() {
                    Name = kvp.Value.Object.GetHashCode().ToString(),
                    Type = DataType.ReferenceObject,
                    OffsetStart = offsetStart,
                    OffsetEnd = _standard.Position,
                    ExplicitType = kvp.Value.Object.GetType().FullName
                };
            }
        }
    }

    public SerializedContainer Close()
    {
        FlushDeferredSerializationList();

        IsSealed = true;

        var container = _standard.Close();

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
        public ISerializable Object;
    }
}
