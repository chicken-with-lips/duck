// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Scene.Components;

public partial struct BoundingSphereComponent : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public BoundingSphereComponent(IDeserializer deserializer, IDeserializationContext context)
    {

        foreach (var entry in deserializer.Index) {
            switch(entry.Name) {

            }
        }
    }
}
