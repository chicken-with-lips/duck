using System.Collections.ObjectModel;

namespace Duck.Serialization;

public class GraphReader
{
    #region Properties

    public ReadOnlyCollection<IndexEntry> Index { get; }

    public ISerializationContext Context
    {
        get => Reader.Context;
    }

    public Reader Reader
    {
        get;
    }

    #endregion

    #region Constructor

    public GraphReader(in ReadOnlyMemory<byte> data, ReadOnlyCollection<IndexEntry> index, ISerializationContext context)
    {
        Index = index;

        Reader = new Reader(data, context);
    }

    #endregion
}
