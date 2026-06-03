namespace Duck.Serialization;

public interface ISerializationFactory
{
    public bool Supports(string typeName);

    public void Serialize(in object value, GraphWriter graphWriter);
    public object Deserialize(string typeName, GraphReader graphReader);
}
