namespace Duck.Serialization;

public interface ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context);
}
