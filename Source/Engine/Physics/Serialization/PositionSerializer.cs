using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class PositionSerializer
{
    public static void Serialize(in Position value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static Position Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new Position();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Value":
                    ret.Value = deserializer.ReadVector3D<AScalar>(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
