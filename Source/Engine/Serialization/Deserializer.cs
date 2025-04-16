using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using ADyn;
using ADyn.Components;
using ADyn.Math;
using ADyn.Shapes;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Duck.Content;
using Silk.NET.Maths;

namespace Duck.Serialization;

public class Deserializer : IDeserializer
{
    #region Properties

    public ReadOnlyCollection<IndexEntry> Index { get; }

    public IDeserializer Root
    {
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

    public AScalar ReadScalar()
    {
#if USE_DOUBLE_PRECISION
        return _reader.ReadDouble();
#else
        return _reader.ReadSingle();
#endif
    }

    public float ReadScalar(long offset)
    {
        _stream.Position = offset;

        return ReadScalar();
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

    // FIXME: boxing?
    private object ReadGeneric<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        switch (Type.GetTypeCode(typeof(T))) {
            case TypeCode.Single:
                return ReadFloat();
            default:
                throw new NotImplementedException();
        }
    }

    private T ReadEnum<T>() where T : struct
    {
        var str = ReadString();

        return (T)Enum.Parse(typeof(T), str);
    }

    public Vector4D<T> ReadVector4D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Vector4D<T>(
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>()
        );
    }

    public Vector4D<T> ReadVector4D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadVector4D<T>();
    }

    public Vector3D<T>? ReadNullOrVector3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var hasValue = ReadBoolean();

        if (!hasValue) {
            return null;
        }

        return new Vector3D<T>(
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>()
        );
    }

    public Vector3D<T>? ReadNullOrVector3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadNullOrVector3D<T>();
    }


    public Vector3D<T> ReadVector3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Vector3D<T>(
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>()
        );
    }

    public Vector3D<T> ReadVector3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadVector3D<T>();
    }

    public Vector2D<T> ReadVector2D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Vector2D<T>(
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>()
        );
    }

    public Vector2D<T> ReadVector2D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadVector2D<T>();
    }

    public Box3D<T> ReadBox3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Box3D<T>(
            ReadVector3D<T>(),
            ReadVector3D<T>()
        );
    }

    public Box3D<T> ReadBox3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadBox3D<T>();
    }

    public Quaternion<T> ReadQuaternion<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Quaternion<T>(
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>(),
            (T)ReadGeneric<T>()
        );
    }

    public Quaternion<T> ReadQuaternion<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadQuaternion<T>();
    }

    public Matrix3X3<T>? ReadNullOrMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        var hasValue = ReadBoolean();

        if (!hasValue) {
            return null;
        }

        return new Matrix3X3<T>(
            ReadVector3D<T>(),
            ReadVector3D<T>(),
            ReadVector3D<T>()
        );
    }

    public Matrix3X3<T>? ReadNullOrMatrix3X3<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadNullOrMatrix3X3<T>();
    }

    public Matrix3X3<T> ReadMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Matrix3X3<T>(
            ReadVector3D<T>(),
            ReadVector3D<T>(),
            ReadVector3D<T>()
        );
    }

    public Matrix3X3<T> ReadMatrix3X3<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadMatrix3X3<T>();
    }


    public Matrix4X4<T> ReadMatrix4X4<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        return new Matrix4X4<T>(
            ReadVector4D<T>(),
            ReadVector4D<T>(),
            ReadVector4D<T>(),
            ReadVector4D<T>()
        );
    }

    public Matrix4X4<T> ReadMatrix4X4<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        _stream.Position = offset;

        return ReadMatrix4X4<T>();
    }

    public AssetReference<T> ReadAssetReference<T>() where T : class, IAsset
    {
        var typeName = ReadString();

        return new AssetReference<T> {
            AssetId = ReadGuid(),
            UniqueId = ReadGuid(),
            IsShared = ReadBoolean(),
        };
    }

    public AssetReference<T> ReadAssetReference<T>(long offset) where T : class, IAsset
    {
        _stream.Position = offset;

        return ReadAssetReference<T>();
    }

    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>() where T : unmanaged, IShape
    {
        var shapeType = ReadString();
        IShape? shape = null;
        var hasShape = ReadBoolean();

        if (hasShape) {
            if (shapeType == typeof(BoxShape).FullName) {
                shape = ReadBoxShape();
            } else if (shapeType == typeof(CapsuleShape).FullName) {
                shape = ReadCapsuleShape();
            } else if (shapeType == typeof(CylinderShape).FullName) {
                shape = ReadCylinderShape();
            } else if (shapeType == typeof(MeshShape).FullName) {
                throw new NotImplementedException();
            } else if (shapeType == typeof(SphereShape).FullName) {
                shape = ReadSphereShape();
            } else if (shapeType == typeof(PlaneShape).FullName) {
                shape = ReadPlaneShape();
            } else {
                throw new NotImplementedException();
            }
        }

        return new RigidBodyDefinition<T> {
            Kind = ReadEnum<RigidBodyKind>(),
            Position = ReadVector3D<AScalar>(),
            Orientation = ReadQuaternion<AScalar>(),
            Mass = (AScalar)ReadGeneric<AScalar>(),
            Inertia = ReadNullOrMatrix3X3<AScalar>(),
            LinearVelocity = ReadVector3D<AScalar>(),
            AngularVelocity = ReadVector3D<AScalar>(),
            CenterOfMass = ReadNullOrVector3D<AScalar>(),
            Gravity = ReadNullOrVector3D<AScalar>(),
            Shape = shape != null ? (T)shape : null,
            Material = ReadNullOrMaterial(),
            CollisionGroup = ReadUInt64(),
            CollisionMask = ReadUInt64(),
            IsPresentation = ReadBoolean(),
            PreventSleeping = ReadBoolean(),
            IsNetworked = ReadBoolean(),
        };
    }

    public BoxShape ReadBoxShape()
    {
        return new BoxShape() {
            HalfExtents = ReadVector3D<AScalar>(),
        };
    }

    public BoxShape ReadBoxShape(long offset)
    {
        _stream.Position = offset;

        return ReadBoxShape();
    }

    public CapsuleShape ReadCapsuleShape()
    {
        return new CapsuleShape() {
            Radius = (AScalar)ReadGeneric<AScalar>(),
            HalfLength = (AScalar)ReadGeneric<AScalar>(),
            Axis = ReadEnum<CoordinateAxis>(),
        };
    }

    public CapsuleShape ReadCapsuleShape(long offset)
    {
        _stream.Position = offset;

        return ReadCapsuleShape();
    }

    public CylinderShape ReadCylinderShape()
    {
        return new CylinderShape() {
            Radius = (AScalar)ReadGeneric<AScalar>(),
            HalfLength = (AScalar)ReadGeneric<AScalar>(),
            Axis = ReadEnum<CoordinateAxis>(),
        };
    }

    public CylinderShape ReadCylinderShape(long offset)
    {
        _stream.Position = offset;

        return ReadCylinderShape();
    }

    public PlaneShape ReadPlaneShape()
    {
        return new PlaneShape() {
            Normal = ReadVector3D<AScalar>(),
            Constant = (AScalar)ReadGeneric<AScalar>(),
        };
    }

    public PlaneShape ReadPlaneShape(long offset)
    {
        _stream.Position = offset;

        return ReadPlaneShape();
    }

    public SphereShape ReadSphereShape()
    {
        return new SphereShape() {
            Radius = (AScalar)ReadGeneric<AScalar>(),
        };
    }

    public SphereShape ReadSphereShape(long offset)
    {
        _stream.Position = offset;

        return ReadSphereShape();
    }

    public Material? ReadNullOrMaterial()
    {
        var hasValue = ReadBoolean();

        if (!hasValue) {
            return null;
        }

        return ReadMaterial();
    }

    public Material? ReadNullOrMaterial(long offset)
    {
        _stream.Position = offset;

        return ReadNullOrMaterial();
    }

    public Material ReadMaterial()
    {
        return new Material {
            Id = ReadUInt16(),
            Restitution = (AScalar)ReadGeneric<AScalar>(),
            Friction = (AScalar)ReadGeneric<AScalar>(),
            SpinFriction = (AScalar)ReadGeneric<AScalar>(),
            RollFriction = (AScalar)ReadGeneric<AScalar>(),
            Stiffness = (AScalar)ReadGeneric<AScalar>(),
            Damping = (AScalar)ReadGeneric<AScalar>(),
        };
    }

    public Material ReadMaterial(long offset)
    {
        _stream.Position = offset;

        return ReadMaterial();
    }

    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>(long offset) where T : unmanaged, IShape
    {
        _stream.Position = offset;

        return ReadRigidBodyDefinition<T>();
    }

    public Entity ReadEntity()
    {
        return new Entity(
            ReadInt32(),
            ReadInt32()
        );
    }

    public Entity ReadEntity(long offset)
    {
        _stream.Position = offset;

        return ReadEntity();
    }

    public EntityReference ReadEntityReference()
    {
        return new EntityReference(
            ReadEntity(),
            ReadInt32()
        );
    }

    public EntityReference ReadEntityReference(long offset)
    {
        _stream.Position = offset;

        return ReadEntityReference();
    }

    /*public T ReadObjectInternal<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator, long? offset, int? objectId)
    {
        if (offset != null) {
            _stream.Position = offset.Value;
        }

        var container = ReadSerializedContainer();
        var deserializer = new Deserializer(container.Data, container.Index, _context, this);
        var context = new DeserializationContext(_context, objectId);

        return objectInstanciator(deserializer, context);
    }*/

    public object ReadObject(string typeName)
    {
        var container = ReadSerializedContainer();
        var deserializer = new Deserializer(container.Data, container.Index, _context, this);

        return Serializer.Deserialize(typeName, deserializer, new DeserializationContext(_context));
    }

    public object ReadObject(string typeName, long offset)
    {
        _stream.Position = offset;

        return ReadObject(typeName);
    }

    public T ReadObject<T>()
    {
        return (T)ReadObject(typeof(T).FullName);
    }

    public T ReadObject<T>(long offset)
    {
        return (T)ReadObject(typeof(T).FullName, offset);
    }

    /*public T ReadObject<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator)
    {
        return ReadObjectInternal(objectInstanciator, null, null);
    }

    public T ReadObject<T>(IDeserializer.ObjectInstanciator<T> objectInstanciator, long offset)
    {
        return ReadObjectInternal(objectInstanciator, offset, null);
    }

    public T ReadObjectReference<T>(IDeserializer.ObjectReferenceInstanciator<T> objectInstanciator, IndexEntry lookup)
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
    }*/

    public List<EntityReference> ReadEntityReferenceList()
    {
        var objectList = new List<EntityReference>();
        var count = ReadInt32();

        for (var i = 0; i < count; i++) {
            objectList.Add(
                ReadEntityReference()
            );
        }

        return objectList;
    }

    public List<EntityReference> ReadEntityReferenceList(long offset)
    {
        _stream.Position = offset;

        return ReadEntityReferenceList();
    }

    public List<EntityReference>? ReadNullOrEntityReferenceList()
    {
        var hasValue = ReadBoolean();

        if (!hasValue) {
            return null;
        }

        var count = ReadInt32();
        var list = new List<EntityReference>(count);

        for (var i = 0; i < count; i++) {
            list.Add(
                ReadEntityReference()
            );
        }

        return list;
    }

    public List<EntityReference>? ReadNullOrEntityReferenceList(long offset)
    {
        _stream.Position = offset;

        return ReadNullOrEntityReferenceList();
    }

    public TContainerType ReadObjectList<TContainerType, TElementType>()
        where TContainerType : class, IList
    {
        var serializedContainer = ReadSerializedContainer();
        var objectList = new List<TElementType>();
        var deserializer = new Deserializer(serializedContainer.Data, serializedContainer.Index, _context, this);

        foreach (var entry in serializedContainer.Index) {
            TElementType obj;

            if (entry.Type == DataType.ReferenceObject) {
                // obj = deserializer.ReadObjectReference((d, c, e) => objectInstanciator(d, c, e), entry);
                throw new NotImplementedException();
            } else {
                obj = (TElementType)deserializer.ReadObject(typeof(TElementType).FullName, entry.OffsetStart);
            }

            objectList.Add(
                obj
            );
        }

        return objectList as TContainerType;
    }

    public TContainerType ReadObjectList<TContainerType, TElementType>(long offset)
        where TContainerType : class, IList
    {
        _stream.Position = offset;

        return ReadObjectList<TContainerType, TElementType>(offset);
    }

/*
    public T1 ReadObjectList<T1, T2>(IDeserializer.NestedObjectInstanciator<T2> objectInstanciator, string containerType, long offset)
        where T1 : class
    {
        _stream.Position = offset;

        return ReadObjectList<T1, T2>(objectInstanciator, containerType);
    }*/

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
