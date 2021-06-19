using System.Runtime.CompilerServices;

namespace Editor;

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Main(string[] args)
    {
        var app = new Editor(true);

        if (app.Initialize()) {
            app.Run();
        }
    }
}
