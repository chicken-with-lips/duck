using Silk.NET.Maths;

namespace Duck.Serialization;

public static class SilkMathWriterExtensions
{
    extension(Writer writer)
    {
        public void Write<T>(Vector4D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.WriteGeneric(value.X);
            writer.WriteGeneric(value.Y);
            writer.WriteGeneric(value.Z);
            writer.WriteGeneric(value.W);
        }

        public void Write<T>(Vector3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.WriteGeneric(value.X);
            writer.WriteGeneric(value.Y);
            writer.WriteGeneric(value.Z);
        }

        public void Write<T>(Vector2D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.WriteGeneric(value.X);
            writer.WriteGeneric(value.Y);
        }

        public void Write<T>(Matrix3X3<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.Write(value.Row1);
            writer.Write(value.Row2);
            writer.Write(value.Row3);
        }

        public void Write<T>(Matrix4X4<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.Write(value.Row1);
            writer.Write(value.Row2);
            writer.Write(value.Row3);
            writer.Write(value.Row4);
        }

        public void Write<T>(Box3D<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.Write(value.Min);
            writer.Write(value.Max);
        }

        public void Write<T>(Quaternion<T> value) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            writer.ThrowIfSealed();

            writer.WriteGeneric(value.X);
            writer.WriteGeneric(value.Y);
            writer.WriteGeneric(value.Z);
            writer.WriteGeneric(value.W);
        }
    }
}
