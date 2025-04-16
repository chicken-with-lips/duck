using ADyn.Components;
using Arch.Core;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class IslandAabbSerializer
{
    public static void Serialize(in IslandAabb value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Aabb", value.Aabb);
    }

    public static IslandAabb Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new IslandAabb();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Aabb":
                    ret.Aabb = deserializer.ReadObject<Aabb>(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
