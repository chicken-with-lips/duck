using Duck.GameHost;

namespace Duck.GameFramework.GameClient
{
    public class GameClientInitializationContext : IGameClientInitializationContext
    {
        public GameClientInitializationContext(IApplication application, bool isHotReload)
        {
            Application = application;
            IsHotReload = isHotReload;
        }

        #region IGameClientInitializationContext

        public IApplication Application { get; }
        public bool IsHotReload { get; }

        #endregion
    }
}
