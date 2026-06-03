namespace Duck.Serialization;

public class SilkMathSerializationFactory : ISerializationFactory
{
    public bool Supports(string typeName)
    {
        switch (typeName) {
            case "Silk.NET.Maths.Box3D<T>":
            case "Silk.NET.Maths.Vector2D<T>":
            case "Silk.NET.Maths.Vector3D<T>":
            case "Silk.NET.MathsVector4D<T>":
            case "Silk.NET.Maths.Matrix3X3<T>":
            case "Silk.NET.Maths.Matrix4X4<T>":
            case "Silk.NET.Maths.Quaternion<T>":
                return true;
        }

        return false;
    }

    public void Serialize(in object value, GraphWriter graphWriter)
    {
        /*switch (value.GetType().FullName) {
            case "Silk.NET.Maths.Box3D<T>":
                graphSerializer.
                break;
            case "Silk.NET.Maths.Vector2D<T>":
                graphSerializer.
                break;
            case "Silk.NET.Maths.Vector3D<T>":
                graphSerializer.
                break;
            case "Silk.NET.MathsVector4D<T>":
                graphSerializer.
                break;
            case "Silk.NET.Maths.Matrix3X3<T>":
                graphSerializer.
                break;
            case "Silk.NET.Maths.Matrix4X4<T>":
                graphSerializer.
                break;
            case "Silk.NET.Maths.Quaternion<T>":
                graphSerializer.
                break;
            default:*/
        throw new NotImplementedException();
        /* }*/
    }

    public object Deserialize(string typeName, GraphReader deserializer)
    {
        throw new NotImplementedException();
    }
}