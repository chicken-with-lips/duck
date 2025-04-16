using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class InertiaSerializer
{
    public static void Serialize(in Inertia value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static Inertia Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new Inertia();
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
