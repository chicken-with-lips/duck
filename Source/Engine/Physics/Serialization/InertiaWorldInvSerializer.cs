using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class InertiaWorldInvSerializer
{
    public static void Serialize(in InertiaWorldInv value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static InertiaWorldInv Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new InertiaWorldInv();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Value":
                    ret.Value = deserializer.ReadMatrix3X3<AScalar>(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
