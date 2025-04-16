using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class OrientationSerializer
{
    public static void Serialize(in Orientation value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static Orientation Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new Orientation();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Value":
                    ret.Value = deserializer.ReadQuaternion<AScalar>(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
