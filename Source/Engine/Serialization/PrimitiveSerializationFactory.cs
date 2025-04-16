namespace Duck.Serialization;

public class PrimitiveSerializationFactory : ISerializationFactory
{
    public bool Supports(string typeName)
    {
        switch (typeName) {
            case "ADyn.Shapes.BoxShape":
            case "ADyn.Shapes.CapsuleShape":
            case "ADyn.Shapes.CylinderShape":
            case "ADyn.Shapes.IShape":
            case "ADyn.Shapes.MeshShape":
            case "ADyn.Shapes.PlaneShape":
            case "ADyn.Shapes.SphereShape":
            case "Arch.Core.EntityReference":
            case "Duck.Content.AssetReference<T>":
            case "Duck.Graphics.Materials.Material":
            case "Silk.NET.Maths.Box3D<T>":
            case "Silk.NET.Maths.Vector2D<T>":
            case "Silk.NET.Maths.Vector3D<T>":
            case "Silk.NET.MathsVector4D<T>":
            case "Silk.NET.Maths.Matrix3X3<T>":
            case "Silk.NET.Maths.Matrix4X4<T>":
            case "Silk.NET.Maths.Quaternion<T>":
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

    public void Serialize(object value, IGraphSerializer graphSerializer, ISerializationContext context)
    {
        /*switch (value.GetType().FullName) {
            case "ADyn.Shapes.BoxShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.CapsuleShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.CylinderShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.IShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.MeshShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.PlaneShape":
                graphSerializer.
                break;
            case "ADyn.Shapes.SphereShape":
                graphSerializer.
                break;
            case "Arch.Core.EntityReference":
                graphSerializer.
                break;
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
                throw new System.NotImplementedException();
       /* }*/
    }

    public object Deserialize(string typeName, IDeserializer deserializer, IDeserializationContext context)
    {
        throw new NotImplementedException();
    }
}
