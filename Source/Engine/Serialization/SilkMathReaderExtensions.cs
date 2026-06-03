using Silk.NET.Maths;

namespace Duck.Serialization;

public static class SilkMathReaderExtensions
{
    extension(Reader reader)
    {
        public Vector4D<T> ReadVector4D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Vector4D<T>(
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>()
            );
        }

        public Vector4D<T> ReadVector4D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadVector4D<T>();
        }

        public Vector3D<T>? ReadNullOrVector3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            var hasValue = reader.ReadBoolean();

            if (!hasValue) {
                return null;
            }

            return new Vector3D<T>(
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>()
            );
        }

        public Vector3D<T>? ReadNullOrVector3D<T>(long offset)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadNullOrVector3D<T>();
        }


        public Vector3D<T> ReadVector3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Vector3D<T>(
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>()
            );
        }

        public Vector3D<T> ReadVector3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadVector3D<T>();
        }

        public Vector2D<T> ReadVector2D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Vector2D<T>(
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>()
            );
        }

        public Vector2D<T> ReadVector2D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadVector2D<T>();
        }

        public Box3D<T> ReadBox3D<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Box3D<T>(
                reader.ReadVector3D<T>(),
                reader.ReadVector3D<T>()
            );
        }

        public Box3D<T> ReadBox3D<T>(long offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadBox3D<T>();
        }

        public Quaternion<T> ReadQuaternion<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Quaternion<T>(
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>(),
                reader.ReadGeneric<T>()
            );
        }

        public Quaternion<T> ReadQuaternion<T>(long offset)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadQuaternion<T>();
        }

        public Matrix3X3<T>? ReadNullOrMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            var hasValue = reader.ReadBoolean();

            if (!hasValue) {
                return null;
            }

            return new Matrix3X3<T>(
                reader.ReadVector3D<T>(),
                reader.ReadVector3D<T>(),
                reader.ReadVector3D<T>()
            );
        }

        public Matrix3X3<T>? ReadNullOrMatrix3X3<T>(long offset)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadNullOrMatrix3X3<T>();
        }

        public Matrix3X3<T> ReadMatrix3X3<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Matrix3X3<T>(
                reader.ReadVector3D<T>(),
                reader.ReadVector3D<T>(),
                reader.ReadVector3D<T>()
            );
        }

        public Matrix3X3<T> ReadMatrix3X3<T>(long offset)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadMatrix3X3<T>();
        }


        public Matrix4X4<T> ReadMatrix4X4<T>() where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new Matrix4X4<T>(
                reader.ReadVector4D<T>(),
                reader.ReadVector4D<T>(),
                reader.ReadVector4D<T>(),
                reader.ReadVector4D<T>()
            );
        }

        public Matrix4X4<T> ReadMatrix4X4<T>(long offset)
            where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            reader.Position = offset;

            return reader.ReadMatrix4X4<T>();
        }
    }
}