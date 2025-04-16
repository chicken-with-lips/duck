using ADyn.Collision;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class IslandTreeResidentSerializer
{
    public static void Serialize(in IslandTreeResident value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Id", value.Id);
    }

    public static IslandTreeResident Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new IslandTreeResident();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Id":
                    ret.Id = deserializer.ReadUInt32(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
