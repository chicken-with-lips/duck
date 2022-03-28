using System.Runtime.CompilerServices;

namespace Game
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            var app = new Game(false);

            if (app.Initialize()) {
                app.Run();
            }
        }
    }
}
