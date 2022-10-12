﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Ecs;

public partial class ComponentPoolCollection : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public ComponentPoolCollection(IDeserializer deserializer, IDeserializationContext context)
    {
        if (context.ObjectId != null) { 
            context.AddObject(context.ObjectId.Value, this);
        }

        foreach (var entry in deserializer.Index) {
            switch(entry.Name) {

            }
        }
    }
}
