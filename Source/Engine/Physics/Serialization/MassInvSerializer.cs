using ADyn;
using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class MassInvSerializer
{
    public static void Serialize(in MassInv value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static MassInv Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new MassInv();
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
