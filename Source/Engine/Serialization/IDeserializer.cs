using System.Collections.ObjectModel;
using ADyn;
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
    public uint ReadUInt32();
    public uint ReadUInt32(long offset);
    public long ReadInt64();
    public long ReadInt64(long offset);
    public float ReadFloat();
    public float ReadFloat(long offset);
    public Single ReadSingle();
    public Single ReadSingle(long offset);
    public bool ReadBoolean();
    public bool ReadBoolean(long offset);
    public byte ReadByte();
    public byte ReadByte(long offset);
    public ReadOnlyMemory<byte> ReadBytes(long count);
    public ReadOnlyMemory<byte> ReadBytes(long count, long offset);
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
    public Matrix4X4<T> ReadMatrix4X4<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public Matrix4X4<T> ReadMatrix4X4<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public EntityReference ReadEntityReference();
    public EntityReference ReadEntityReference(long offset);
    public AssetReference<T> ReadAssetReference<T>() where T : class, IAsset;
    public AssetReference<T> ReadAssetReference<T>(long offset) where T : class, IAsset;
    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>() where T : unmanaged, IShape;
    public RigidBodyDefinition<T> ReadRigidBodyDefinition<T>(long offset) where T : unmanaged, IShape;

    public T ReadObject<T>(ObjectInstanciator<T> objectInstanciator)
        where T : ISerializable;

    public T ReadObject<T>(ObjectInstanciator<T> objectInstanciator, long offset)
        where T : ISerializable;

    public T ReadObjectReference<T>(ObjectReferenceInstanciator<T> objectInstanciator, IndexEntry lookup)
        where T : ISerializable;

    public T1 ReadObjectList<T1, T2>(NestedObjectInstanciator<T2> objectInstanciator, string containerType)
        where T1 : class
        where T2 : ISerializable;

    public T1 ReadObjectList<T1, T2>(NestedObjectInstanciator<T2> objectInstanciator, string containerType, long offset)
        where T1 : class
        where T2 : ISerializable;

    public delegate T ObjectInstanciator<T>(IDeserializer deserializer, IDeserializationContext context);

    public delegate T ObjectReferenceInstanciator<T>(IDeserializer deserializer, IDeserializationContext context, IndexEntry entry);

    public delegate T NestedObjectInstanciator<T>(IDeserializer deserializer, IDeserializationContext context, IndexEntry entry);
}
