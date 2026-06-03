using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.LowLevel.Jagged;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class WorldSerializationFactory : ISerializationFactory
{
    public World? World { get; set; }

    public bool Supports(string typeName)
    {
        switch (typeName) {
            case "Arch.Core.Archetype":
            case "Arch.Core.Chunk":
            case "Arch.Core.ComponentType":
            case "Arch.Core.Entity":
            case "Arch.Core.Signature":
            case "Arch.Core.World":
                return true;
        }

        return false;
    }

    public void Serialize(in object value, GraphWriter graphWriter)
    {
        switch (value) {
            case Archetype archetype:
                Serialize(archetype, graphWriter);

                break;
            case Chunk:
                throw new SerializationException("Chunks must be serialized via Serialize(Archetype)");

            case ComponentType componentType:
                Serialize(componentType, graphWriter);

                break;
            case Entity entity:
                Serialize(entity, graphWriter);

                break;
            case Signature signature:
                Serialize(signature, graphWriter);

                break;
            case World world:
                Serialize(world, graphWriter);

                break;
            default:
                throw new SerializationException("Unknown value");
        }
    }

    public object Deserialize(string typeName, GraphReader graphReader)
    {
        switch (typeName) {
            case "Arch.Core.Archetype":
                return DeserializeArchetype(graphReader);
            case "Arch.Core.ComponentType":
                return DeserializeComponentType(graphReader);
            case "Arch.Core.Chunk":
                throw new SerializationException("Chunks must be deserialized via DeserializeArchetype");
            case "Arch.Core.Entity":
                return DeserializeEntity(graphReader);
            case "Arch.Core.Signature":
                return DeserializeSignature(graphReader);
            case "Arch.Core.World":
                return DeserializeWorld(graphReader);
            default:
                throw new SerializationException("Unknown value: " + typeName);
        }
    }

    public void Serialize(in World value, GraphWriter graphWriter)
    {
        // Write important metadata
        graphWriter.Writer.Write((uint)value.BaseChunkSize);
        graphWriter.Writer.Write((uint)value.BaseChunkEntityCount);

        // Write slots
        SerializeEntityData(value.GetEntityDataArray(), graphWriter);

        // Write recycled entity ids
        var recycledEntityIDs = value.GetRecycledEntityIds();

        graphWriter.Writer.Write(recycledEntityIDs.Count);

        for (var index = 0; index < recycledEntityIDs.Count; index++) {
            graphWriter.Writer.Write(recycledEntityIDs[index].Item1);
            graphWriter.Writer.Write(recycledEntityIDs[index].Item2);
        }

        // Write archetypes
        graphWriter.Writer.Write((uint)value.Archetypes.Count);

        foreach (var archetype in value) {
            Serializer.Serialize(archetype, graphWriter);
        }
    }

    private void SerializeEntityData(in JaggedArray<EntityData> value, GraphWriter graphWriter)
    {
        // Write length/capacity and items
        graphWriter.Writer.Write(value.Capacity);

        for (var index = 0; index < value.Capacity; index++) {
            // Write chunk index
            graphWriter.Writer.Write((uint)value[index].Slot.ChunkIndex);

            // Write entity index
            graphWriter.Writer.Write((uint)value[index].Slot.Index);
        }
    }

    public void Serialize(in Archetype value, GraphWriter graphWriter)
    {
        var types = value.Signature;
        var chunks = value.Chunks;

        Serializer.Serialize(types, graphWriter);
        graphWriter.Writer.Write(value.GetLookupArray());
        graphWriter.Writer.Write((uint)value.ChunkCount);

        for (var index = 0; index < value.ChunkCount; index++) SerializeChunk(chunks[index], types, graphWriter);
    }

    public void Serialize(in Signature value, GraphWriter graphWriter)
    {
        // Write count and types
        graphWriter.Writer.Write((uint)value.Count);

        foreach (var type in value.Components) {
            Serializer.Serialize(type, graphWriter);
        }
    }

    public void Serialize(in ComponentType value, GraphWriter graphWriter)
    {
        graphWriter.Writer.Write((uint)value.Id);
        graphWriter.Writer.Write((uint)value.ByteSize);
    }

    private void SerializeChunk(in Chunk value, Signature signature, GraphWriter graphWriter)
    {
        // Write size
        graphWriter.Writer.Write((uint)value.Count);

        // Write capacity
        graphWriter.Writer.Write((uint)value.Capacity);

        // Write entities
        graphWriter.Writer.Write(value.Entities.Length);

        foreach (var t in value.Entities) {
            Serializer.Serialize(t, graphWriter);
        }

        // Persist arrays as an array...
        foreach (var type in signature.Components) {
            // Write array itself
            var array = value.GetArray(type);

            graphWriter.Writer.Write(array.GetType().GetElementType()!.FullName!);
            graphWriter.Writer.Write(array.Length);

            for (var index = 0; index < array.Length; index++) {
                var componentWriter = new GraphWriter(graphWriter.Context);
                Serializer.Serialize(array.GetValue(index)!, componentWriter);
                graphWriter.Writer.Write(componentWriter.Close());
            }
        }
    }

    public void Serialize(in Entity value, GraphWriter graphWriter)
    {
        graphWriter.Writer.Write(value.Id);
        graphWriter.Writer.Write(value.Version);
    }

    private Entity DeserializeEntity(GraphReader graphReader)
    {
        var id = graphReader.Reader.ReadInt32();
        var version = graphReader.Reader.ReadInt32();

        return DangerousEntityExtensions.CreateEntityStruct(id, World!.Id, version);
    }

    private Chunk DeserializeChunk(GraphReader graphReader, Signature signature, int[] lookupArray, Archetype archetype)
    {
        // Read chunk size
        var size = graphReader.Reader.ReadUInt32();

        // Read chunk capacity
        var capacity = graphReader.Reader.ReadUInt32();

        // Read entities
        var entityCount = graphReader.Reader.ReadInt32();
        var entities = new Entity[entityCount];

        for (var index = 0; index < entityCount; index++) entities[index] = Serializer.Deserialize<Entity>(graphReader);

        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)capacity, lookupArray, signature);
        entities.CopyTo(chunk.Entities, 0);
        chunk.SetSize((int)size);

        // Updating World.EntityInfoStorage to their new archetype
        for (var index = 0; index < size; index++) {
            ref var entity = ref chunk.Entity(index);
            entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, World!.Id, entity.Version);
            World.SetArchetype(entity, archetype);
        }

        // Persist arrays as an array...
        foreach (var _ in signature.Components) {
            // Read array of the type

            var componentTypeName = graphReader.Reader.ReadString();

            // var componentType = Type.GetType(componentTypeName);
            var componentCount = graphReader.Reader.ReadInt32();
            var componentType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(componentTypeName, false, false)).FirstOrDefault(t => t != null) ?? throw new SerializationException($"Could not resolve component type '{componentTypeName}' in any loaded assembly");

            var array = Array.CreateInstance(componentType, componentCount);
            var chunkArray = chunk.GetArray(componentType);

            for (var index = 0; index < componentCount; index++) {
                var componentContainer = graphReader.Reader.ReadSerializedContainer();
                var componentReader = new GraphReader(componentContainer.Data, componentContainer.Index, graphReader.Context);

                array.SetValue(Serializer.Deserialize(componentType.FullName!, componentReader), index);
            }

            Array.Copy(array, chunkArray, (int)size);
        }

        return chunk;
    }

    private ComponentType DeserializeComponentType(GraphReader graphReader)
    {
        var id = graphReader.Reader.ReadUInt32();
        var byteSize = graphReader.Reader.ReadUInt32();

        return new ComponentType((int)id, (int)byteSize);
    }

    private Signature DeserializeSignature(GraphReader graphReader)
    {
        // Read count
        var count = graphReader.Reader.ReadUInt32();

        // Read types
        var componentTypes = new ComponentType[count];

        for (var index = 0; index < count; index++)
            componentTypes[index] = Serializer.Deserialize<ComponentType>(graphReader);

        return new Signature(componentTypes);
    }

    private Archetype DeserializeArchetype(GraphReader graphReader)
    {
        // Types
        var types = Serializer.Deserialize<Signature>(graphReader);

        // Archetype lookup array
        var lookupArray = graphReader.Reader.ReadInt32Array();

        // Archetype chunk size and list
        var chunkSize = graphReader.Reader.ReadUInt32();

        // Create archetype
        var chunks = new List<Chunk>((int)chunkSize);
        var archetype = DangerousArchetypeExtensions.CreateArchetype(World!.BaseChunkSize, World!.BaseChunkEntityCount, types);
        archetype.Chunks.Clear(true);
        archetype.SetCount((int)chunkSize - 1);

        // Deserialize each chunk and put it into the archetype.
        var entities = 0;

        for (var index = 0; index < chunkSize; index++) {
            var chunk = DeserializeChunk(graphReader, types, lookupArray, archetype);
            chunks.Add(chunk);
            entities += chunk.Count;
        }

        archetype.SetChunks(chunks);
        archetype.SetEntities(entities);

        return archetype;
    }

    public World DeserializeWorld(GraphReader graphReader)
    {
        // Read important metadata
        var baseChunkSize = graphReader.Reader.ReadUInt32();
        var baseChunkEntityCount = graphReader.Reader.ReadUInt32();

        var world = World ?? World.Create((int)baseChunkSize, minimumAmountOfEntitiesPerChunk: (int)baseChunkEntityCount);

        World = world;

        // Read slots
        var slots = DeserializeEntityData(graphReader);

        //Read recycled entity ids
        var recycledEntityIdCount = graphReader.Reader.ReadInt32();
        var recycledEntityIDs = new List<(int, int)>(recycledEntityIdCount);

        for (var index = 0; index < recycledEntityIdCount; index++)
            recycledEntityIDs.Add((graphReader.Reader.ReadInt32(), graphReader.Reader.ReadInt32()));

        // Forward values to the world
        world.SetRecycledEntityIds(recycledEntityIDs);
        world.SetEntityDataArray(slots);
        world.EnsureCapacity(slots.Capacity);

        // Read archetypes
        var size = graphReader.Reader.ReadInt32();
        List<Archetype> archetypes = new();

        for (var index = 0; index < size; index++) {
            var archetype = Serializer.Deserialize<Archetype>(graphReader);
            archetypes.Add(archetype);
        }

        // Set archetypes
        world.SetArchetypes(archetypes);

        World = null;

        return world;
    }

    private JaggedArray<EntityData> DeserializeEntityData(GraphReader graphReader)
    {
        const int cpuL1CacheSize = 16_384;

        var capacity = graphReader.Reader.ReadInt32();
        var jaggedArray = new JaggedArray<EntityData>(cpuL1CacheSize / Unsafe.SizeOf<EntityData>(), new EntityData(null!, new Slot(-1, -1), -1), capacity);

        for (var index = 0; index < capacity; index++) {
            // Read chunk index and entity index
            var chunkIndex = graphReader.Reader.ReadUInt32();
            var entityIndex = graphReader.Reader.ReadUInt32();

            jaggedArray.Add(index, new EntityData(null!, new Slot((int)entityIndex, (int)chunkIndex), 0));
        }

        return jaggedArray;
    }
}
