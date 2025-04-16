using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class LinearVelocitySerializer
{
    public static void Serialize(in LinearVelocity value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static LinearVelocity Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new LinearVelocity();
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
