using System.Collections.ObjectModel;
using System.Numerics;
using ADyn;
using ADyn.Components;
using ADyn.Shapes;
using Arch.Core;
using Duck.Content;
using Duck.Serialization.Exception;
using Silk.NET.Maths;

namespace Duck.Serialization;

public class DefaultBasicSerializer : IBasicSerializer
{
    #region Properties

    public bool IsSealed { get; private set; }
    public long Position => _stream.Position;

    #endregion

    #region Members

    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;

    #endregion

    public DefaultBasicSerializer()
    {
        _stream = new MemoryStream();
        _writer = new BinaryWriter(_stream);
    }

    public void Write(in string value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in int value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in ushort value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in long value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in ulong value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in float value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in bool value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in byte value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    public void Write(in byte[] value)
    {
        ThrowIfSealed();

        _writer.Write(value);
    }

    private void WriteGeneric<T>(in T value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        switch (Type.GetTypeCode(typeof(T))) {
            case TypeCode.Single:
                Write(Convert.ToSingle(value));
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void Write<T>(in Vector4D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        WriteGeneric(value.X);
        WriteGeneric(value.Y);
        WriteGeneric(value.Z);
        WriteGeneric(value.W);
    }

    public void Write<T>(in Vector3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        WriteGeneric(value.X);
        WriteGeneric(value.Y);
        WriteGeneric(value.Z);
    }

    public void Write<T>(in Vector2D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        WriteGeneric(value.X);
        WriteGeneric(value.Y);
    }

    public void Write<T>(in Matrix3X3<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        Write(value.Row1);
        Write(value.Row2);
        Write(value.Row3);
    }

    public void Write<T>(in Matrix4X4<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        Write(value.Row1);
        Write(value.Row2);
        Write(value.Row3);
        Write(value.Row4);
    }

    public void Write<T>(in Box3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        Write(value.Min);
        Write(value.Max);
    }

    public void Write<T>(in Quaternion<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        ThrowIfSealed();

        WriteGeneric(value.X);
        WriteGeneric(value.Y);
        WriteGeneric(value.Z);
        WriteGeneric(value.W);
    }

    public void Write(in Guid value)
    {
        ThrowIfSealed();

        Write(value.ToByteArray());
    }

    public void Write<T>(in AssetReference<T> value) where T : class, IAsset
    {
        ThrowIfSealed();

        throw new NotImplementedException();
        // Write(value);
    }

    public void Write(in Enum value)
    {
        ThrowIfSealed();

        _writer.Write(value.ToString());
    }

    public void Write<TShapeType>(in RigidBodyDefinition<TShapeType> value) where TShapeType : unmanaged, IShape
    {
        ThrowIfSealed();

        Write(typeof(TShapeType).FullName);
        Write(value.Shape.HasValue);
        if (value.Shape.HasValue) {
            Write(value.Shape.Value);
        }
        Write(value.Kind.ToString());
        Write(value.Position);
        Write(value.Orientation);
        Write(value.Mass);
        Write(value.Inertia.HasValue);
        if (value.Inertia.HasValue) {
            Write(value.Inertia.Value);
        }
        Write(value.LinearVelocity);
        Write(value.AngularVelocity);
        Write(value.CenterOfMass.HasValue);
        if (value.CenterOfMass.HasValue) {
            Write(value.CenterOfMass.Value);
        }
        Write(value.Gravity.HasValue);
        if (value.Gravity.HasValue) {
            Write(value.Gravity.Value);
        }
        Write(value.Material.HasValue);
        if (value.Material.HasValue) {
            Write(value.Material.Value);
        }
        Write(value.CollisionGroup);
        Write(value.CollisionMask);
        Write(value.IsPresentation);
        Write(value.PreventSleeping);
        Write(value.IsNetworked);
    }

    public void Write(in Material value)
    {
        Write(value.Id);
        Write(value.Restitution);
        Write(value.Friction);
        Write(value.SpinFriction);
        Write(value.RollFriction);
        Write(value.Stiffness);
        Write(value.Damping);
    }

    public void Write(in IShape value)
    {
        ThrowIfSealed();

        if (value is BoxShape boxShape) {
            Write(boxShape);
        } else if (value is CapsuleShape capsuleShape) {
            Write(capsuleShape);
        } else if (value is CylinderShape cylinderShape) {
            Write(cylinderShape);
        } else if (value is MeshShape meshShape) {
            Write(meshShape);
        } else if (value is SphereShape sphereShape) {
            Write(sphereShape);
        } else if (value is PlaneShape planeShape) {
            Write(planeShape);
        } else {
            throw new NotImplementedException();
        }
    }

    public void Write(in BoxShape value)
    {
        ThrowIfSealed();

        Write(value.HalfExtents);
    }

    public void Write(in CapsuleShape value)
    {
        ThrowIfSealed();

        Write(value.Radius);
        Write(value.HalfLength);
        Write(value.Axis.ToString());
    }

    public void Write(in CylinderShape value)
    {
        ThrowIfSealed();

        Write(value.Radius);
        Write(value.HalfLength);
        Write(value.Axis.ToString());
    }

    public void Write(in MeshShape value)
    {
        throw new NotImplementedException();
    }

    public void Write(in PlaneShape value)
    {
        ThrowIfSealed();

        Write(value.Normal);
        Write(value.Constant);
    }

    public void Write(in SphereShape value)
    {
        ThrowIfSealed();

        Write(value.Radius);
    }

    public void Write<T>(in Material value) where T : class, IAsset
    {
        ThrowIfSealed();

        Write(value.Id);
        Write(value.Restitution);
        Write(value.Friction);
        Write(value.SpinFriction);
        Write(value.RollFriction);
        Write(value.Stiffness);
        Write(value.Damping);
    }

    public void Write(EntityReference value)
    {
        ThrowIfSealed();

        Write(value.Entity.Id);
        Write(value.Version);
    }

    public SerializedContainer Close()
    {
        ThrowIfSealed();

        IsSealed = true;

        return new SerializedContainer() {
            Index = new ReadOnlyCollection<IndexEntry>(new List<IndexEntry>()),
            Data = _stream.ToArray(),
        };
    }

    private void ThrowIfSealed()
    {
        if (IsSealed) {
            throw new SerializationException("Serializer is sealed");
        }
    }
}
