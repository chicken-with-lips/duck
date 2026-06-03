namespace Duck.Serialization;

public static class DuckReaderExtensions
{
    extension(Reader reader)
    {
        public DScalar ReadScalar()
        {
#if USE_DOUBLE_PRECISION
        return _reader.ReadDouble();
#else
            return reader.ReadSingle();
#endif
        }

        public DScalar ReadScalar(long offset)
        {
            reader.Position = offset;

            return reader.ReadScalar();
        }
    }
}