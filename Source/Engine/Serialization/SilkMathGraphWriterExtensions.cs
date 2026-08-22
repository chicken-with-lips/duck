using Silk.NET.Maths;

namespace Duck.Serialization;

public static class SilkMathGraphWriterExtensions
{
    extension(GraphWriter writer)
    {
        public void Write(string name, Vector4D<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Vector4D, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Vector3D<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Vector3D, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Vector2D<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Vector2D, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Box3D<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Box3D, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Quaternion<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Quaternion, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Matrix3X3<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Matrix3X3, offsetStart, writer.Writer.Position);
        }

        public void Write(string name, Matrix4X4<DScalar> value)
        {
            writer.ThrowIfSealed();

            var offsetStart = writer.Writer.Position;

            writer.Writer.Write(value);
            writer.PushIndex(name, DataType.Matrix4X4, offsetStart, writer.Writer.Position);
        }
    }
}
