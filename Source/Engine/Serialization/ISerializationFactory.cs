namespace Duck.Serialization;

public interface ISerializationFactory
{
    public bool Supports(string typeName);

    public void Serialize(object value, IGraphSerializer graphSerializer, ISerializationContext context);
    public object Deserialize(string typeName, IDeserializer deserializer, IDeserializationContext context);
}
