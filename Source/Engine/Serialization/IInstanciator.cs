namespace Duck.Serialization;

public interface IInstanciator
{
    public bool CanInstanciate(string typeName);
    public object Instanciate(string typeName, IDeserializer deserializer, IDeserializationContext context);

    public object Instanciate<T>(string typeName, IDeserializer deserializer, IDeserializationContext context)
        where T : struct;
}
