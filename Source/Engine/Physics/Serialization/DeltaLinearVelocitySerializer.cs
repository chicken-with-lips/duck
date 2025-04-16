using ADyn.Components;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class DeltaLinearVelocitySerializer
{
    public static void Serialize(in DeltaLinearVelocity value, IGraphSerializer serializer, ISerializationContext context)
    {
        serializer.Write("Value", value.Value);
    }

    public static DeltaLinearVelocity Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        var ret = new DeltaLinearVelocity();
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
