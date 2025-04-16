using ADyn;
using Duck.Serialization;

namespace Duck.Physics.Serialization;

public static class RowCacheSerializer
{
    public static void Serialize(in RowCache value, IGraphSerializer serializer, ISerializationContext context)
    {
        
        throw new NotImplementedException();
        // serializer.WriteNullOr("Rows", value.Rows);
        // serializer.WriteNullOr("ConstraintRowCount", value.ConstraintRowCount);
        // serializer.WriteNullOr("Flags", value.Flags);
        // serializer.WriteNullOr("Friction", value.Friction);
        // serializer.WriteNullOr("Rolling", value.Rolling);
        // serializer.WriteNullOr("Spinning", value.Spinning);
    }

    public static RowCache Deserialize(IDeserializer deserializer, IDeserializationContext context)
    {
        throw new NotImplementedException();
        // var ret = new RowCache();
        // foreach (var entry in deserializer.Index) {
        //     switch (entry.Name) {
        //         case "Rows":
        //             ret.Rows = deserializer.ReadNullOrEntityReferenceList(entry.OffsetStart);
        //             break;
        //         case "ConstraintRowCount":
        //             ret.ConstraintRowCount = deserializer.ReadNullOrEntityReferenceList(entry.OffsetStart);
        //             break;
        //         case "SleepTimestamp":
        //             ret.SleepTimestamp = deserializer.ReadNullOrDouble(entry.OffsetStart);
        //             break;
        //     }
        // }
        //
        // return ret;
    }
}
