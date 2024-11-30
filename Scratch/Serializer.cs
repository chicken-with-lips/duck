using Arch.Core;
using MessagePack;
using MessagePack.Formatters;
using System.Buffers;
using Utf8Json;
using Utf8Json.Resolvers;
using DateTimeFormatter = Utf8Json.Formatters.DateTimeFormatter;
using NullableDateTimeFormatter = Utf8Json.Formatters.NullableDateTimeFormatter;

namespace Scratch;

/// <summary>
///     The <see cref="ArchSerializer"/> interface
///     represents an interface with shared methods to (de)serialize worlds and entities.
///     <remarks>It might happen that the serialized object is too large to fit into a regular c# byte-array. In this case use the <see cref="IBufferWriter{T}"/>-API.</remarks>
/// </summary>
public interface IArchSerializer
{
    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="byte"/>-array.
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    byte[] Serialize(World world, Entity entity);

    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="Stream"/> e.g. a File or existing array.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    void Serialize(Stream stream, World world, Entity entity);

    /// <summary>
    ///     Serializes an <see cref="Entity"/> to a <see cref="IBufferWriter{T}"/> e.g. a File or existing array.
    /// </summary>
    /// <param name="writer">The <see cref="IBufferWriter{T}"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    void Serialize(IBufferWriter<byte> writer, World world, Entity entity);

    /// <summary>
    ///     Deserializes an <see cref="Entity"/> from its bytes to an real <see cref="Entity"/> in a <see cref="World"/>.
    ///     <remarks>The new <see cref="Entity.Id"/> and <see cref="Entity.WorldId"/> will differ.</remarks>
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns></returns>
    Entity Deserialize(World world, byte[] entity);

    /// <summary>
    ///     Deserializes an <see cref="Entity"/> from its bytes to an real <see cref="Entity"/> in a <see cref="World"/>.
    ///     <remarks>The new <see cref="Entity.Id"/> and <see cref="Entity.WorldId"/> will differ.</remarks>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    /// <returns></returns>
    Entity Deserialize(Stream stream, World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="byte"/>-array.
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    byte[] Serialize(World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    void Serialize(Stream stream, World world);

    /// <summary>
    ///     Serializes a <see cref="World"/> to a <see cref="IBufferWriter{T}"/>.
    /// </summary>
    /// <param name="writer">The <see cref="IBufferWriter{T}"/>.</param>
    /// <param name="world">The <see cref="World"/>.</param>
    void Serialize(IBufferWriter<byte> writer, World world);

    /// <summary>
    ///     Deserializes a byte-array into a <see cref="World"/>.
    /// </summary>
    /// <param name="world">The <see cref="World"/> as an byte-array.</param>
    /// <returns>The new <see cref="World"/>.</returns>
    World Deserialize(byte[] world);

    /// <summary>
    ///     Deserializes a byte-array into a <see cref="World"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <returns>The new <see cref="World"/>.</returns>
    World Deserialize(Stream stream);
}

/// <summary>
///     The <see cref="ArchBinarySerializer"/> class
///     represents a binary serializer for arch to (de)serialize single entities and whole worlds by binary. 
/// </summary>
public class ArchBinarySerializer : IArchSerializer
{
    /// <summary>
    ///     The default formatters used to (de)serialize the <see cref="World"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _formatters =
    {
        new WorldFormatter(),
        new ArchetypeFormatter(),
        new ChunkFormatter(),
        new ArrayFormatter(),
        new GraphIndexEntryFormatter(),
        new ComponentTypeFormatter(),
        new EntitySlotFormatter(),
        new EntityFormatter(),
        new JaggedArrayFormatter<int>(-1),
        new JaggedArrayFormatter<(int,int)>((-1,-1)),
        new JaggedArrayFormatter<(Archetype,(int,int))>((null, (-1,-1)))
    };

    /// <summary>
    ///     The default formatters used to (de)serialize a single <see cref="Entity"/>.
    /// </summary>
    private readonly IMessagePackFormatter[] _singleEntityFormatters =
    {
        new ComponentTypeFormatter(),
        new SingleEntityFormatter()
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
            MessagePack.Resolvers.CompositeResolver.Create(
                _formatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver>
                {
                    MessagePack.Resolvers.BuiltinResolver.Instance,
                    MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance
                }
            )
        );

        _singleEntityOptions = MessagePackSerializerOptions.Standard.WithResolver(
            MessagePack.Resolvers.CompositeResolver.Create(
                _singleEntityFormatters.Concat(custFormatters).ToList(),
                new List<IFormatterResolver>
                {
                    MessagePack.Resolvers.BuiltinResolver.Instance,
                    MessagePack.Resolvers.ContractlessStandardResolverAllowPrivate.Instance
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
        return MessagePackSerializer.Serialize(world, _options); ;
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
