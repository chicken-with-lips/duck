﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Ecs;

public partial class EntityPool : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public EntityPool(IDeserializer deserializer, IDeserializationContext context)
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