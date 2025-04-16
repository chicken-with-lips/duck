using System.Reflection;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.LowLevel.Jagged;
using Duck.ECS.Components;
using Duck.Serialization;

namespace Duck.Graphics.Serialization;

public static class InMemoryWorldCloner
{
    public static World Copy(World sourceWorld)
    {
        var serializer = new ArchBinarySerializer();
        var data = serializer.Serialize(sourceWorld);

        return serializer.Deserialize(data);
    }
}
