using System.Collections.ObjectModel;
using Arch.Core;
using Duck.Content;
using Silk.NET.Maths;

namespace Duck.Serialization;

public interface ISerializer
{
    public bool IsSealed { get; }

    public SerializedContainer Close();
}

public interface IStandardSerializer : ISerializer
{
    public void Write(in string value);
    public void Write(in int value);
    public void Write(in long value);
    public void Write(in float value);
    public void Write(in bool value);
    public void Write(in byte value);
    public void Write(in byte[] value);
    public void Write(in Vector3D<float> value);
    public void Write(in Vector2D<float> value);
    public void Write(in Box3D<float> value);
    public void Write(in Quaternion<float> value);
    public void Write(in Guid value);
    public void Write<T>(in AssetReference<T> value)
        where T : class, IAsset;
    public void Write(EntityReference value);
}

public interface IGraphSerializer : ISerializer
{
    public IGraphSerializer Root { get; }
    public IGraphSerializer? Parent { get; }

    public void Write(string name, in string value);
    public void Write(string name, in int value);
    public void Write(string name, in float value);
    public void Write(string name, in bool value);
    public void Write(string name, in Vector3D<float> value);
    public void Write(string name, in Vector2D<float> value);
    public void Write(string name, in Box3D<float> value);
    public void Write(string name, in Quaternion<float> value);
    public void Write<T>(string name, in AssetReference<T> value)
        where T : class, IAsset;
    public void Write(string name, EntityReference value);
    public void Write(string name, in ISerializable value);
    public void Write(string name, in ISerializable[] value, string containerType);
    public int TrackReferenceAndDeferSerialization(in ISerializable value);
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
    Float = 2,
    Int32 = 3,
    String = 4,
    Vector2D = 5,
    Vector3D = 6,
    Box3D = 7,
    Quaternion = 8,
    ValueObject = 9,
    ReferenceObject = 10,
}
