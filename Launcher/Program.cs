using System.Runtime.CompilerServices;
using Duck;
using Duck.Contracts.Logging;
using Launcher.ClientHost;

namespace Launcher
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            var app = new Application(false);
            app.AddSubsystem(new ClientHostSubsystem(app, app.GetSubsystem<ILogSubsystem>()));

            if (app.Init()) {
                app.Run();
            }
        }
    }
}
