using ADyn.Components;
using Arch.Core;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class IslandTagSerializer
{
    public static void Serialize(in IslandTag value, IGraphSerializer serializer, ISerializationContext context)
    {
    }

    public static IslandTag Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        return new IslandTag();
    }
}
