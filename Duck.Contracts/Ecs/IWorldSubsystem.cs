using System;
using System.Buffers;
using Duck.Contracts;

namespace Duck.Ecs
{
    public interface IWorldSubsystem : IApplicationSubsystem
    {
        public IWorld[] Worlds { get; }

        public IWorld Create();

        public void Serialize(IWorld world, IBufferWriter<byte> destination);
        public IWorld Deserialize(ReadOnlyMemory<byte> data);
    }
}
