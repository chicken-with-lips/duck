using System.Collections.ObjectModel;
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
    public long ReadInt64();
    public long ReadInt64(long offset);
    public float ReadFloat();
    public float ReadFloat(long offset);
    public bool ReadBoolean();
    public bool ReadBoolean(long offset);
    public byte ReadByte();
    public byte ReadByte(long offset);
    public ReadOnlyMemory<byte> ReadBytes(long count);
    public ReadOnlyMemory<byte> ReadBytes(long count, long offset);
    public Vector3D<float> ReadVector3D();
    public Vector3D<float> ReadVector3D(long offset);
    public Vector2D<float> ReadVector2D();
    public Vector2D<float> ReadVector2D(long offset);
    public Box3D<float> ReadBox3D();
    public Box3D<float> ReadBox3D(long offset);
    public Quaternion<float> ReadQuaternion();
    public Quaternion<float> ReadQuaternion(long offset);

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
