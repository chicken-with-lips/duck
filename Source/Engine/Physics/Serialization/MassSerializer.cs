using ADyn;
using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class MassSerializer
{
    public static void Serialize(in Mass value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static Mass Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new Mass();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Value":
                    ret.Value = deserializer.ReadScalar(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
