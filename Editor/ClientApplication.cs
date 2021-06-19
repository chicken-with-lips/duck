using Duck;
using Duck.Contracts.Logging;
using Editor.ClientHost;

namespace Editor
{
    public class ClientApplication : BaseApplication
    {
        public ClientApplication(bool isEditor) : base(isEditor)
        {
        }

        protected override void RegisterSubsystems()
        {
            base.RegisterSubsystems();

            AddSubsystem(new ClientHostSubsystem(this, GetSubsystem<ILogSubsystem>()));
        }
    }
}
