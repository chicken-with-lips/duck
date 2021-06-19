using System.Buffers;

namespace Duck.Ecs.Serialization;

public interface IWorldSerializer
{
    public void Serialize(IWorld world, IBufferWriter<byte> destination);
    public IWorld Deserialize(ReadOnlyMemory<byte> input);
}

public interface IWorldSerializedData
{
}
