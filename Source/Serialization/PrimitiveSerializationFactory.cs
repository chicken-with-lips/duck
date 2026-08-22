namespace Duck.Serialization;

public class PrimitiveSerializationFactory : ISerializationFactory
{
    public bool Supports(string typeName)
    {
        switch (typeName) {
            case "System.Boolean":
            case "System.Byte":
            case "System.Byte[]":
            case "System.Double":
            case "System.Enum":
            case "System.Guid":
            case "System.Int32":
            case "System.Int64":
            case "System.UInt16":
            case "System.UInt64":
            case "System.Single":
            case "System.String":
                return true;
        }

        return false;
    }

    public void Serialize(object value, GraphWriter graphWriter)
    {
        /*switch (value.GetType().FullName) {
            case "Duck.Content.AssetReference<T>":
                graphSerializer.
                break;
            case "Duck.Graphics.Materials.Material":
                graphSerializer.
                break;
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
            case "System.Boolean":
                graphSerializer.
                break;
            case "System.Byte":
                graphSerializer.
                break;
            case "System.Byte[]":
                graphSerializer.
                break;
            case "System.Double":
                graphSerializer.
                break;
            case "System.Enum":
                graphSerializer.
                break;
            case "System.Guid":
                graphSerializer.
                break;
            case "System.Int32":
                graphSerializer.
                break;
            case "System.Int64":
                graphSerializer.
                break;
            case "System.UInt16":
                graphSerializer.
                break;
            case "System.UInt64":
                graphSerializer.
                break;
            case "System.Single":
                graphSerializer.
                break;
            case "System.String":
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