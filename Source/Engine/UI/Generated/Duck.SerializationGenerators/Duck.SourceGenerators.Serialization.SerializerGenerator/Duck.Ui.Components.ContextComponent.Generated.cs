﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Ui.Components;

public partial struct ContextComponent : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public ContextComponent(IDeserializer deserializer, IDeserializationContext context)
    {

        foreach (var entry in deserializer.Index) {
            switch(entry.Name) {

            }
        }
    }
}