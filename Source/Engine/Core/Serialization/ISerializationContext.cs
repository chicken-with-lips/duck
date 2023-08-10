namespace Duck.Serialization;

public interface ISerializationContext
{
    public bool IsHotReload { get; }
    public bool HasObject(int objectId);
    public ISerializable GetObject(int objectId);
    public void AddObject(int objectId, ISerializable obj);
}

public interface IDeserializationContext : ISerializationContext
{
    public int? ObjectId { get; }
}
