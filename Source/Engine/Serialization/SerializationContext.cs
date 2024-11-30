using System.Collections.Concurrent;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class SerializationContext : ISerializationContext
{
    #region Properties

    public bool IsHotReload { get; }

    #endregion

    #region Members

    private readonly ConcurrentDictionary<int, object> _sharedObjects = new();

    #endregion

    #region Methods

    public SerializationContext(bool isHotReload)
    {
        IsHotReload = isHotReload;
    }

    public bool HasObject(int objectId)
    {
        return _sharedObjects.ContainsKey(objectId);
    }

    public object GetObject(int objectId)
    {
        if (_sharedObjects.TryGetValue(objectId, out var data)) {
            return data;
        }

        throw new SerializationException("Shared object could not be found.");
    }

    public void AddObject(int objectId, object obj)
    {
        if (HasObject(objectId)) {
            throw new SerializationException("Shared object has already been added.");
        }

        _sharedObjects.TryAdd(objectId, obj);
    }

    #endregion
}

public class DeserializationContext : IDeserializationContext
{
    #region Properties

    public int? ObjectId { get; }
    public bool IsHotReload => _serializationContext.IsHotReload;

    #endregion

    #region Members

    private readonly ISerializationContext _serializationContext;

    #endregion

    #region Methods

    public DeserializationContext(ISerializationContext serializationContext, int? objectId = null)
    {
        ObjectId = objectId;

        _serializationContext = serializationContext;
    }


    public object GetObject(int objectId)
    {
        return _serializationContext.GetObject(objectId);
    }

    public void AddObject(int objectId, object obj)
    {
        _serializationContext.AddObject(objectId, obj);
    }

    public bool HasObject(int objectId)
    {
        return _serializationContext.HasObject(objectId);
    }

    #endregion
}
