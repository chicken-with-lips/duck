﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Scene;

public partial class Scene : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public Scene(IDeserializer deserializer, IDeserializationContext context)
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
