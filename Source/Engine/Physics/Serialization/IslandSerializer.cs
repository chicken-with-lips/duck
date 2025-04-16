using ADyn.Components;
using Arch.Core;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class IslandSerializer
{
    public static void Serialize(in Island value, IGraphSerializer serializer, ISerializationContext context)
    {
        throw new NotImplementedException();
        /*serializer.WriteNullOr("Nodes", value.Nodes);
        serializer.WriteNullOr("Edges", value.Edges);
        serializer.WriteNullOr("SleepTimestamp", value.SleepTimestamp);*/
    }

    public static Island Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        throw new NotImplementedException();
        /*var ret = new Island();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Nodes":
                    ret.Nodes = deserializer.ReadNullOrEntityReferenceList(entry.OffsetStart);
                    break;
                case "Edges":
                    ret.Edges = deserializer.ReadNullOrEntityReferenceList(entry.OffsetStart);
                    break;
                case "SleepTimestamp":
                    ret.SleepTimestamp = deserializer.ReadNullOrDouble(entry.OffsetStart);
                    break;
            }
        }

        return ret;*/
    }
}