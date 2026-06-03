using Arch.Core;
using Arch.Core.Extensions.Dangerous;

namespace Duck.Serialization;

public static class ArchReaderExtensions
{
    extension(Reader reader)
    {
        public Entity ReadEntity()
        {
            return DangerousEntityExtensions.CreateEntityStruct(
                reader.ReadInt32(),
                0,
                reader.ReadInt32()
            );
        }

        public Entity ReadEntity(long offset)
        {
            reader.Position = offset;

            return reader.ReadEntity();
        }
    }
}