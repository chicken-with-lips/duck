﻿// <auto-generated />

using Duck.Serialization;
using Duck.Serialization.Exception;

namespace Duck.Ui.Components;

public partial struct UserInterfaceComponent : ISerializable
{
    public void Serialize(IGraphSerializer serializer, ISerializationContext context)
    {

    }

    public UserInterfaceComponent(IDeserializer deserializer, IDeserializationContext context)
    {

        foreach (var entry in deserializer.Index) {
            switch(entry.Name) {

            }
        }
    }
}