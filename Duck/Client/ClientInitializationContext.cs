using Duck.Contracts;
using Duck.Game;

namespace Duck.Client
{
    public class ClientInitializationContext : IClientInitializationContext
    {
        public ClientInitializationContext(IApplication application, bool isHotReload)
        {
            Application = application;
            IsHotReload = isHotReload;
        }

        #region IClientInitializationContext

        public IApplication Application { get; }

        public bool IsHotReload { get; }

        #endregion
    }
}
