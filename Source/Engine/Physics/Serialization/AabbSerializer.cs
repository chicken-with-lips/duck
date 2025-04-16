using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class AabbSerializer
{
    public static void Serialize(in Aabb value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Min", value.Min);
        serializer.Write("Max", value.Max);
    }

    public static Aabb Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new Aabb();
        foreach (var entry in deserializer.Index) {
            switch (entry.Name) {
                case "Min":
                    ret.Min = deserializer.ReadVector3D<AScalar>(entry.OffsetStart);
                    break;
                case "Max":
                    ret.Max = deserializer.ReadVector3D<AScalar>(entry.OffsetStart);
                    break;
            }
        }

        return ret;
    }
}
