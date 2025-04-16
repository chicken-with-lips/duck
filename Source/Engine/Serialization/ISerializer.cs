using System.Collections.ObjectModel;
using ADyn;
using ADyn.Components;
using ADyn.Constraints;
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

public interface IPrimitiveSerializer : ISerializer
{
    public void Write(in string value);
    public void Write(in int value);
    public void Write(in ushort value);
    public void Write(in long value);
    public void Write(in ulong value);
    public void Write(in float value);
    public void Write(in double value);
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
    public void Write(in Enum value);
    public void Write<T>(in AssetReference<T> value)
        where T : class, IAsset;

    public void Write<T>(in Material value)
        where T : class, IAsset;
    public void Write<TShapeType>(in RigidBodyDefinition<TShapeType> value)
        where TShapeType : unmanaged, IShape;
    public void Write(in Material value);
    public void Write(in IShape value);
    public void Write(in BoxShape value);
    public void Write(in CapsuleShape value);
    public void Write(in CylinderShape value);
    public void Write(in MeshShape value);
    public void Write(in PlaneShape value);
    public void Write(in SphereShape value);

    public void Write(in EntityReference value);
}

public interface IGraphSerializer : ISerializer
{
    public IGraphSerializer Root { get; }
    public IGraphSerializer? Parent { get; }
    
    public void Write(string name, in string value);
    public void Write(string name, in int value);
    public void Write(string name, in float value);
    public void WriteNullOr(string name, in float? value);
    public void Write(string name, in double value);
    public void WriteNullOr(string name, in double? value);
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

    public void Write(string name, in EntityReference value);
    public void Write(string name, in object value);
    public int TrackReferenceAndDeferSerialization(in object value);
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
    RigidBodyDefinition = 13,
    ValueObject = 14,
    ReferenceObject = 15,
    GenericList = 16,
    List = 17,
    NullOrList = 18,
    NullOrFloat = 19,
    NullOrDouble = 20,
    EntityReference = 21,
}