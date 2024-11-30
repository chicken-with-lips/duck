using System.Buffers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Arch.LowLevel.Jagged;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

// using Utf8Json;

namespace Duck.Serialization.Arch;

/// <summary>
///     The <see cref="ArchBinarySerializer"/> class
///     represents a binary serializer for arch to (de)serialize single entities and whole worlds by binary. 
/// </summary>
public class ArchBinarySerializer
{
    /// <summary>
    ///     The default formatters used to (de)serialize the <see cref="World"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _formatters = {
        new WorldFormatter(),
        new ArchetypeFormatter(),
        new ChunkFormatter(),
        new ArrayFormatter(),
        new GraphIndexEntryFormatter(),
        new ComponentTypeFormatter(),
        new EntitySlotFormatter(),
        new EntityFormatter(),
        new GraphIndexEntryFormatter(),
        new JaggedArrayFormatter<int>(-1),
        new JaggedArrayFormatter<(int, int)>((-1, -1)),
        new JaggedArrayFormatter<(Archetype, (int, int))>((null, (-1, -1)))
    };

    /// <summary>
    ///     The default formatters used to (de)serialize a single <see cref="Entity"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _singleEntityFormatters = {
        new ComponentTypeFormatter(),
        new SingleEntityFormatter(),
    };

    /// <summary>
    ///     The standard <see cref="MessagePackSerializerOptions"/> for world (de)serialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    ///     The standard <see cref="MessagePackSerializerOptions"/> for single entity (de)serialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _singleEntityOptions;

    /// <summary>
    ///     The static constructor gets called during compile time to setup the serializer. 
    /// </summary>
    public ArchBinarySerializer(params IMessagePackFormatter[] custFormatters)
    {
        // Register all important jsonformatters 
        _options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                _formatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver> {
                    BuiltinResolver.Instance,
                    ContractlessStandardResolverAllowPrivate.Instance
                }
            )
        );

        _singleEntityOptions = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                _singleEntityFormatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver> {
                    BuiltinResolver.Instance,
                    ContractlessStandardResolverAllowPrivate.Instance
                }
            )
        );
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world, Entity entity)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Serialize(entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world, Entity entity)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        MessagePackSerializer.Serialize(stream, entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world, Entity entity)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        MessagePackSerializer.Serialize(writer, entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public Entity Deserialize(World world, byte[] entity)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Deserialize<Entity>(entity, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public Entity Deserialize(Stream stream, World world)
    {
        (_singleEntityFormatters[1] as SingleEntityFormatter)!.EntityWorld = world;
        return MessagePackSerializer.Deserialize<Entity>(stream, _singleEntityOptions);
    }

    /// <inheritdoc/>
    public byte[] Serialize(World world)
    {
        return MessagePackSerializer.Serialize(world, _options);
        ;
    }

    /// <inheritdoc/>
    public void Serialize(Stream stream, World world)
    {
        MessagePackSerializer.Serialize(stream, world, _options);
    }

    /// <inheritdoc/>
    public void Serialize(IBufferWriter<byte> writer, World world)
    {
        MessagePackSerializer.Serialize(writer, world, _options);
    }

    /// <inheritdoc/>
    public World Deserialize(byte[] world)
    {
        return MessagePackSerializer.Deserialize<World>(world, _options);
    }

    /// <inheritdoc/>
    public World Deserialize(Stream stream)
    {
        return MessagePackSerializer.Deserialize<World>(stream, _options);
    }
}

/// <summary>
///     The <see cref="SingleEntityFormatter"/> class
///     is a <see cref="IJsonFormatter"/> to (de)serialize a single <see cref="Entity"/>to or from json.
/// </summary>
internal class SingleEntityFormatter : IMessagePackFormatter<Entity>
{
    /// <summary>
    ///     The <see cref="EntityWorld"/> the entity belongs to. 
    /// </summary>
    internal World EntityWorld { get; set; }

    public void Serialize(ref MessagePackWriter writer, Entity value, MessagePackSerializerOptions options)
    {
        // Write id
        writer.WriteInt32(value.Id);

#if !PURE_ECS
        // Write world
        writer.WriteInt32(value.WorldId);
#endif

        // Write size
        var componentTypes = EntityWorld.GetComponentTypes(value);
        writer.WriteInt32(componentTypes.Length);

        // Write components
        foreach (ref var type in componentTypes.AsSpan()) {
            // Write type
            MessagePackSerializer.Serialize(ref writer, type, options);

            // Write component
            var cmp = EntityWorld.Get(value, type);
            MessagePackSerializer.Serialize(ref writer, cmp, options);
        }
    }

    public Entity Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read id
        var entityId = reader.ReadInt32();

#if !PURE_ECS
        // Read world id
        var worldId = reader.ReadInt32();
#endif

        // Read size
        var size = reader.ReadInt32();
        var components = new object[size];

        // Read components
        for (var index = 0; index < size; index++) {
            // Read type
            var type = MessagePackSerializer.Deserialize<ComponentType>(ref reader, options);
            var cmp = MessagePackSerializer.Deserialize(type, ref reader, options);
            components[index] = cmp;
        }

        // Create the entity
        var entity = EntityWorld.Create();
        EntityWorld.AddRange(entity, components.AsSpan());
        return entity;
    }
}

/// <summary>
///     The <see cref="EntityFormatter"/> class
///     is a formatter that (de)serializes <see cref="Entity"/> structs. 
/// </summary>
internal class EntityFormatter : IMessagePackFormatter<Entity>
{
    internal int WorldId { get; set; }

    public void Serialize(ref MessagePackWriter writer, Entity value, MessagePackSerializerOptions options)
    {
        writer.WriteInt32(value.Id);
    }

    public Entity Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read id
        var id = reader.ReadInt32();
        return DangerousEntityExtensions.CreateEntityStruct(id, WorldId);
    }
}

/// <summary>
///     The <see cref="ArrayFormatter"/> class
///     is a <see cref="IJsonFormatter{Array}"/> to (de)serialize <see cref="Array"/>s to or from json.
/// </summary>
internal class ArrayFormatter : IMessagePackFormatter<Array>
{
    public void Serialize(ref MessagePackWriter writer, Array value, MessagePackSerializerOptions options)
    {
        var serializationContext = new SerializationContext(false);
        var graphSerializer = new GraphSerializer(serializationContext);

        var type = value.GetType().GetElementType();

        // Write type and size
        MessagePackSerializer.Serialize(ref writer, type, options);
        MessagePackSerializer.Serialize(ref writer, value.Length, options);

        for (var index = 0; index < value.Length; index++) {
            var obj = value.GetValue(index);

            graphSerializer.Write(index.ToString(), obj);
        }

        var res = graphSerializer.Close();

        MessagePackSerializer.Serialize(ref writer, res.Data, options);
        MessagePackSerializer.Serialize(ref writer, res.Index, options);
    }

    public Array Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Write type and size
        var type = MessagePackSerializer.Deserialize<Type>(ref reader, options);
        var size = reader.ReadUInt32();

        // Create array
        var array = Array.CreateInstance(type, size);

        var data = MessagePackSerializer.Deserialize<byte[]>(ref reader, options);
        var indices = MessagePackSerializer.Deserialize<IndexEntry[]>(ref reader, options);

        var context = new DeserializationContext(new SerializationContext(false));
        var deserializer = new Deserializer(data, new ReadOnlyCollection<IndexEntry>(indices), context);


        for (var i = 0; i < size; i++) {
            var entry = indices[i];
            var obj = deserializer.ReadObject(entry.ExplicitType);
            array.SetValue(obj, i);
        }

        return array;
    }
}

internal class GraphIndexEntryFormatter : IMessagePackFormatter<IndexEntry>
{
    public void Serialize(ref MessagePackWriter writer, IndexEntry value, MessagePackSerializerOptions options)
    {
        MessagePackSerializer.Serialize(ref writer, value.Name, options);
        MessagePackSerializer.Serialize(ref writer, value.Type, options);
        MessagePackSerializer.Serialize(ref writer, value.OffsetStart, options);
        MessagePackSerializer.Serialize(ref writer, value.OffsetEnd, options);
        MessagePackSerializer.Serialize(ref writer, value.ExplicitType, options);
    }

    public IndexEntry Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var entry = new IndexEntry();
        entry.Name = MessagePackSerializer.Deserialize<string>(ref reader, options);
        entry.Type = MessagePackSerializer.Deserialize<DataType>(ref reader, options);
        entry.OffsetStart = MessagePackSerializer.Deserialize<long>(ref reader, options);
        entry.OffsetEnd = MessagePackSerializer.Deserialize<long>(ref reader, options);
        entry.ExplicitType = MessagePackSerializer.Deserialize<string?>(ref reader, options);

        return entry;
    }
}

/// <summary>
///     The <see cref="JaggedArrayFormatter{T}"/> class
///     (de)serializes a <see cref="JaggedArray{T}"/>.
/// </summary>
/// <typeparam name="T">The type stored in the <see cref="JaggedArray{T}"/>.</typeparam>
internal class JaggedArrayFormatter<T> : IMessagePackFormatter<JaggedArray<T>>
{
    private const int CpuL1CacheSize = 16_000;
    private readonly T _filler;

    public JaggedArrayFormatter(T filler)
    {
        _filler = filler;
    }

    public void Serialize(ref MessagePackWriter writer, JaggedArray<T> value, MessagePackSerializerOptions options)
    {
        // Write length/capacity and items
        writer.WriteInt32(value.Capacity);
        for (var index = 0; index < value.Capacity; index++) {
            var item = value[index];
            MessagePackSerializer.Serialize(ref writer, item, options);
        }
    }

    public JaggedArray<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var capacity = reader.ReadInt32();
        var jaggedArray = new JaggedArray<T>(CpuL1CacheSize / Unsafe.SizeOf<T>(), _filler, capacity);

        for (var index = 0; index < capacity; index++) {
            var item = MessagePackSerializer.Deserialize<T>(ref reader, options);
            jaggedArray.Add(index, item);
        }

        return jaggedArray;
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
internal class ComponentTypeFormatter : IMessagePackFormatter<ComponentType>
{
    public void Serialize(ref MessagePackWriter writer, ComponentType value, MessagePackSerializerOptions options)
    {
        // Write id
        writer.WriteUInt32((uint)value.Id);

        // Write bytesize
        writer.WriteUInt32((uint)value.ByteSize);
    }

    public ComponentType Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var id = reader.ReadUInt32();
        var bytesize = reader.ReadUInt32();

        return new ComponentType((int)id, (int)bytesize);
    }
}

/// <summary>
///     The <see cref="ComponentTypeFormatter"/> class
///     is a <see cref="IJsonFormatter{ComponentType}"/> to (de)serialize <see cref="ComponentType"/>s to or from json.
/// </summary>
internal partial class EntitySlotFormatter : IMessagePackFormatter<(Archetype, (int, int))>
{
    public void Serialize(ref MessagePackWriter writer, (Archetype, (int, int)) value,
        MessagePackSerializerOptions options)
    {
        // Write chunk index
        writer.WriteUInt32((uint)value.Item2.Item1);

        // Write entity index
        writer.WriteUInt32((uint)value.Item2.Item2);
    }

    public (Archetype, (int, int)) Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read chunk index and entity index
        var chunkIndex = reader.ReadUInt32();
        var entityIndex = reader.ReadUInt32();

        return (null, ((int)chunkIndex, (int)entityIndex));
    }
}

/// <summary>
///     The <see cref="WorldFormatter"/> class
///     is a <see cref="IJsonFormatter{World}"/> to (de)serialize <see cref="World"/>s to or from json.
/// </summary>
internal partial class WorldFormatter : IMessagePackFormatter<World>
{
    public void Serialize(ref MessagePackWriter writer, World value, MessagePackSerializerOptions options)
    {
        // Write entity info
        MessagePackSerializer.Serialize(ref writer, value.GetVersions(), options);

        // Write slots
        MessagePackSerializer.Serialize(ref writer, value.GetSlots(), options);

        //Write recycled entity ids
        var recycledEntityIDs = value.GetRecycledEntityIds();
        MessagePackSerializer.Serialize(ref writer, recycledEntityIDs, options);

        // Write archetypes
        writer.WriteUInt32((uint)value.Archetypes.Count);
        foreach (var archetype in value) {
            MessagePackSerializer.Serialize(ref writer, archetype, options);
        }
    }

    public World Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Create world and setup formatter
        var world = World.Create();
        var archetypeFormatter = options.Resolver.GetFormatter<Archetype>() as ArchetypeFormatter;
        var entityFormatter = options.Resolver.GetFormatter<Entity>() as EntityFormatter;
        entityFormatter.WorldId = world.Id;
        archetypeFormatter.World = world;

        // Read versions
        var versions = MessagePackSerializer.Deserialize<JaggedArray<int>>(ref reader, options);

        // Read slots
        var slots = MessagePackSerializer.Deserialize<JaggedArray<(Archetype, (int, int))>>(ref reader, options);

        //Read recycled entity ids
        var recycledEntityIDs = MessagePackSerializer.Deserialize<List<(int, int)>>(ref reader, options);

        // Forward values to the world
        world.SetVersions(versions);
        world.SetRecycledEntityIds(recycledEntityIDs);
        world.SetSlots(slots);
        world.EnsureCapacity(versions.Capacity);

        // Read archetypes
        var size = reader.ReadInt32();
        List<Archetype> archetypes = new();

        for (var index = 0; index < size; index++) {
            var archetype = archetypeFormatter.Deserialize(ref reader, options);
            archetypes.Add(archetype);
        }

        // Set archetypes
        world.SetArchetypes(archetypes);
        return world;
    }
}

/// <summary>
///     The <see cref="ArchetypeFormatter"/> class
///     is a <see cref="IJsonFormatter{Archetype}"/> to (de)serialize <see cref="Archetype"/>s to or from json.
/// </summary>
internal partial class ArchetypeFormatter : IMessagePackFormatter<Archetype>
{
    /// <summary>
    ///     The <see cref="World"/> which is being used by this formatter during serialisation/deserialisation. 
    /// </summary>
    internal World World { get; set; }

    public void Serialize(ref MessagePackWriter writer, Archetype value, MessagePackSerializerOptions options)
    {
        // Setup formatters
        var types = value.Types;
        var chunks = value.Chunks;
        var chunkFormatter = options.Resolver.GetFormatter<Chunk>() as ChunkFormatter;
        chunkFormatter.Types = types;

        // Write type array
        MessagePackSerializer.Serialize(ref writer, types, options);

        // Write lookup array
        MessagePackSerializer.Serialize(ref writer, value.GetLookupArray(), options);

        // Write chunk size
        writer.WriteUInt32((uint)value.ChunkCount);

        // Write chunks 
        for (var index = 0; index < value.ChunkCount; index++) {
            ref var chunk = ref chunks[index];
            chunkFormatter.Serialize(ref writer, chunk, options);
        }
    }

    public Archetype Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var chunkFormatter = options.Resolver.GetFormatter<Chunk>() as ChunkFormatter;

        // Types
        var types = MessagePackSerializer.Deserialize<ComponentType[]>(ref reader, options);

        // Archetype lookup array
        var lookupArray = MessagePackSerializer.Deserialize<int[]>(ref reader, options);

        // Archetype chunk size and list
        var chunkSize = reader.ReadUInt32();

        // Create archetype
        var chunks = new List<Chunk>((int)chunkSize);
        var archetype = DangerousArchetypeExtensions.CreateArchetype(types.ToArray());
        archetype.SetSize((int)chunkSize);

        // Pass types and lookup array to the chunk formatter for saving performance and memory
        chunkFormatter.World = World;
        chunkFormatter.Archetype = archetype;
        chunkFormatter.Types = types;
        chunkFormatter.LookupArray = lookupArray;

        // Deserialise each chunk and put it into the archetype. 
        var entities = 0;
        for (var index = 0; index < chunkSize; index++) {
            var chunk = chunkFormatter.Deserialize(ref reader, options);
            chunks.Add(chunk);
            entities += chunk.Size;
        }

        archetype.SetChunks(chunks);
        archetype.SetEntities(entities);
        return archetype;
    }
}

/// <summary>
///     The <see cref="ChunkFormatter"/> class
///     is a <see cref="IJsonFormatter{Chunk}"/> to (de)serialize <see cref="Chunk"/>s to or from json.
/// </summary>
internal class ChunkFormatter : IMessagePackFormatter<Chunk>
{
    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    internal World World { get; set; }

    /// <summary>
    ///     The <see cref="Archetype"/> the current (de)serialized <see cref="Chunk"/> belongs to.
    ///     Since chunks do not know this, we need to pass this information along it. 
    /// </summary>
    internal Archetype Archetype { get; set; }

    /// <summary>
    ///     The types used in the <see cref="Chunk"/> in each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    internal ComponentType[] Types { get; set; } = Array.Empty<ComponentType>();

    /// <summary>
    ///     The lookup array used by each <see cref="Chunk"/> (de)serialized by this formatter.
    ///     <remarks>Since <see cref="Chunk"/> does not have a reference to them and its controlled by its <see cref="Archetype"/>.</remarks>
    /// </summary>
    internal int[] LookupArray { get; set; } = Array.Empty<int>();

    public void Serialize(ref MessagePackWriter writer, Chunk value, MessagePackSerializerOptions options)
    {
        // Write size
        writer.WriteUInt32((uint)value.Size);

        // Write capacity
        writer.WriteUInt32((uint)value.Capacity);

        // Write entitys
        MessagePackSerializer.Serialize(ref writer, value.Entities, options);

        // Persist arrays as an array...
        for (var index = 0; index < Types.Length; index++) {
            ref var type = ref Types[index];

            // Write array itself
            var array = value.GetArray(type);
            MessagePackSerializer.Serialize(ref writer, array, options);
        }
    }

    public Chunk Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        // Read chunk size
        var size = reader.ReadUInt32();

        // Read chunk size
        var capacity = reader.ReadUInt32();

        // Read entities
        var entities = MessagePackSerializer.Deserialize<Entity[]>(ref reader, options);

        // Create chunk
        var chunk = DangerousChunkExtensions.CreateChunk((int)capacity, LookupArray, Types);
        entities.CopyTo(chunk.Entities, 0);
        chunk.SetSize((int)size);

        // Updating World.EntityInfoStorage to their new archetype
        for (var index = 0; index < size; index++) {
            ref var entity = ref chunk.Entity(index);
            entity = DangerousEntityExtensions.CreateEntityStruct(entity.Id, World.Id);
            World.SetArchetype(entity, Archetype);
        }

        // Persist arrays as an array...
        for (var index = 0; index < Types.Length; index++) {
            // Read array of the type
            var array = MessagePackSerializer.Deserialize<Array>(ref reader, options);
            var chunkArray = chunk.GetArray(array.GetType().GetElementType());
            Array.Copy(array, chunkArray, (int)size);
        }

        return chunk;
    }
}
