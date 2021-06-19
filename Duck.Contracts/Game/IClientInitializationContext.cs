using Duck.Contracts;

namespace Duck.Game
{
    public interface IClientInitializationContext
    {
        public IApplication Application { get; }
        public bool IsHotReload { get; }
    }
}
