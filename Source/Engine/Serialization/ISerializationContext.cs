namespace Duck.Serialization;

public interface ISerializationContext
{
    public bool IsHotReload { get; }
    public bool HasObject(int objectId);
    public object GetObject(int objectId);
    public void AddObject(int objectId, object obj);
}

public interface IDeserializationContext : ISerializationContext
{
    public int? ObjectId { get; }
}
