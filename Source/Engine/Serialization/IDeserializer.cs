using System.Collections;
using System.Collections.ObjectModel;
using ADyn;
using ADyn.Components;
using ADyn.Shapes;
using Arch.Core;
using Duck.Content;
using Silk.NET.Maths;

namespace Duck.Serialization;

public interface IDeserializer
{
    public IDeserializer Root { get; }
    public IDeserializer? Parent { get; }

    public ReadOnlyCollection<IndexEntry> Index { get; }

    public string ReadString();
    public string ReadString(long offset);
    public int ReadInt32();
    public int ReadInt32(long offset);
    public ushort ReadUInt16();
    public ushort ReadUInt16(long offset);
    public uint ReadUInt32();
    public uint ReadUInt32(long offset);
    public ulong ReadUInt64();
    public ulong ReadUInt64(long offset);
    public long ReadInt64();
    public long ReadInt64(long offset);
    public float ReadScalar();
    public float ReadScalar(long offset);
    public float ReadFloat();
    public float ReadFloat(long offset);
    public double ReadDouble();
    public double ReadDouble(long offset);
    public double? ReadNullOrDouble();
    public double? ReadNullOrDouble(long offset);
    public bool ReadBoolean();
    public bool ReadBoolean(long offset);
    public byte ReadByte();
    public byte ReadByte(long offset);
    public ReadOnlyMemory<byte> ReadBytes(long count);
    public ReadOnlyMemory<byte> ReadBytes(long count, long offset);
    public Guid ReadGuid();
    public Guid ReadGuid(long offset);
    public Vector4D<T> ReadVector4D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Vector4D<T> ReadVector4D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Vector3D<T> ReadVector3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Vector3D<T> ReadVector3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Vector2D<T> ReadVector2D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Vector2D<T> ReadVector2D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Box3D<T> ReadBox3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Box3D<T> ReadBox3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Quaternion<T> ReadQuaternion<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Quaternion<T> ReadQuaternion<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix3X3<T>? ReadNullOrMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;

    public Matrix3X3<T>? ReadNullOrMatrix3X3<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix3X3<T> ReadMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix3X3<T> ReadMatrix3X3<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix4X4<T> ReadMatrix4X4<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix4X4<T> ReadMatrix4X4<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public AssetReference<T> ReadAssetReference<T>() where T : class, IAsset;
    public AssetReference<T> ReadAssetReference<T>(long offset) where T : class, IAsset;
    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>() where T : unmanaged, IShape;
    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>(long offset) where T : unmanaged, IShape;
    public BoxShape ReadBoxShape();
    public BoxShape ReadBoxShape(long offset);
    public CapsuleShape ReadCapsuleShape();
    public CapsuleShape ReadCapsuleShape(long offset);
    public CylinderShape ReadCylinderShape();
    public CylinderShape ReadCylinderShape(long offset);
    public PlaneShape ReadPlaneShape();
    public PlaneShape ReadPlaneShape(long offset);
    public SphereShape ReadSphereShape();
    public SphereShape ReadSphereShape(long offset);
    public Material ReadMaterial();
    public Material ReadMaterial(long offset);
    public EntityReference ReadEntityReference();
    public EntityReference ReadEntityReference(long offset);
    public List<EntityReference> ReadEntityReferenceList();
    public List<EntityReference> ReadEntityReferenceList(long offset);
    public List<EntityReference>? ReadNullOrEntityReferenceList();
    public List<EntityReference>? ReadNullOrEntityReferenceList(long offset);
    public Entity ReadEntity();
    public Entity ReadEntity(long offset);

    public object ReadObject(string typeName);
    public object ReadObject(string typeName, long offset);

    public T ReadObject<T>();
    public T ReadObject<T>(long offset);

    public TContainerType ReadObjectList<TContainerType, TElementType>()
        where TContainerType : class, IList;
    public TContainerType ReadObjectList<TContainerType, TElementType>(long offset)
        where TContainerType : class, IList;

    /*public T ReadObject<T>(ObjectInstanciator<T> objectInstanciator);

    public T ReadObject<T>(ObjectInstanciator<T> objectInstanciator, long offset);

    public T ReadObjectReference<T>(ObjectReferenceInstanciator<T> objectInstanciator, IndexEntry lookup);

    public delegate T ObjectInstanciator<T>(IDeserializer deserializer, IDeserializationContext context);

    public delegate T ObjectReferenceInstanciator<T>(IDeserializer deserializer, IDeserializationContext context, IndexEntry entry);

    public delegate T NestedObjectInstanciator<T>(IDeserializer deserializer, IDeserializationContext context, IndexEntry entry);*/
}
