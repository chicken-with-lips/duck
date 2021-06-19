using System.Runtime.CompilerServices;
using Duck;
using Duck.Contracts.Logging;
using Editor.ClientHost;

namespace Editor
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            var app = new ClientApplication(true);

            if (app.Init()) {
                app.Run();
            }
        }
    }
}
