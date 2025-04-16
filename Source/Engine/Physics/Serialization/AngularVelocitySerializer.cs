using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class AngularVelocitySerializer
{
    public static void Serialize(in AngularVelocity value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static AngularVelocity Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new AngularVelocity();
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
