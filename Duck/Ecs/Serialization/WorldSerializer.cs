using System;
using System.Buffers;
using MessagePack;
using MessagePack.Resolvers;

namespace Duck.Ecs.Serialization
{
    public class WorldSerializer : IWorldSerializer
    {
        #region IWorldSerializer

        public void Serialize(IWorld world, IBufferWriter<byte> destination)
        {
            if (!(world is World tmp)) {
                throw new Exception("Tried to serialize an incompatible world");
            }

            MessagePackSerializer.Serialize(destination, tmp);
        }

        public IWorld Deserialize(ReadOnlyMemory<byte> input)
        {
            // var worldData = input as WorldSerializedData;

            // if (null == worldData) {
            // throw new Exception("Unsupported input type");
            // }

            var world = (World) MessagePackSerializer.Deserialize(
                typeof(World),
                input,
                GetOptions()
            );
            //
            // ((EntityPool) world.EntityPool).World = world;
            //
            // foreach (var filter in world.Filters) {
            // ((FilterBase) filter).World = world;
            // }
            //
            // for (var i = 0; i < world.EntityPool.Count; i++) {
            // ((Entity) world.EntityPool.Get(i)).World = world;
            // }

            // var world = (World) SerializationUtility.DeserializeValue<World>(input, DataFormat.Binary);

            return world;
        }

        private MessagePackSerializerOptions GetOptions()
        {
            var resolver = CompositeResolver.Create(
                ContractlessStandardResolverAllowPrivate.Instance,
                TypelessContractlessStandardResolver.Instance
            );

            return StandardResolverAllowPrivate.Options
                .WithResolver(resolver)
                .WithOmitAssemblyVersion(true)
                .WithAllowAssemblyVersionMismatch(true);
        }

        #endregion
    }

    public class WorldSerializedData : IWorldSerializedData
    {
        public byte[] Blob { get; }

        public WorldSerializedData(byte[] data)
        {
            Blob = data;
        }
    }
}
