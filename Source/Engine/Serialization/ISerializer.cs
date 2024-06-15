using System.Collections.ObjectModel;
using ADyn;
using ADyn.Components;
using ADyn.Shapes;
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
    public void Write<T>(in Vector4D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Vector3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Vector2D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Matrix4X4<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Matrix3X3<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Box3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write<T>(in Quaternion<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>;
    public void Write(in Guid value);
    public void Write<T>(in AssetReference<T> value)
        where T : class, IAsset;

    public void Write<T>(in Material value)
        where T : class, IAsset;
    public void Write<TShapeType>(in RigidBodyDefinition<TShapeType> value)
        where TShapeType : unmanaged, IShape;
    public void Write(in BoxShape value);
    public void Write(in CapsuleShape value);
    public void Write(in CylinderShape value);
    public void Write(in MeshShape value);
    public void Write(in PlaneShape value);
    public void Write(in SphereShape value);

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
    public void Write(string name, in Vector4D<float> value);
    public void Write(string name, in Vector3D<float> value);
    public void Write(string name, in Vector2D<float> value);
    public void Write(string name, in Box3D<float> value);
    public void Write(string name, in Quaternion<float> value);
    public void Write(string name, in Matrix3X3<float> value);
    public void Write(string name, in Matrix4X4<float> value);
    public void Write<T>(string name, in AssetReference<T> value)
        where T : class, IAsset;
    public void Write<TShapeType>(string name, in RigidBodyDefinition<TShapeType> value)
        where TShapeType : unmanaged, IShape;
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
    Vector4D = 7,
    Box3D = 8,
    Quaternion = 9,
    Matrix3X3 = 10,
    Matrix4X4 = 11,
    RigidBodyDefinition = 12,
    ValueObject = 13,
    ReferenceObject = 14,
}
