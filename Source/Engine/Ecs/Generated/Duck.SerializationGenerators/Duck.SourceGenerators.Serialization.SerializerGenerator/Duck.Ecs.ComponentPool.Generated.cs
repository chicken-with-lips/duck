﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Ecs;

public partial class ComponentPool<T> : ISerializable, IComponentPool<T> where T : struct
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public ComponentPool(IDeserializer deserializer, IDeserializationContext context)
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
