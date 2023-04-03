using System.Runtime.CompilerServices;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL;

namespace Game
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            var app = new Game(
                new StandardPlatform(),
                new OpenGLRenderSystem(),
                false
            );

            if (app.Initialize()) {
                app.Run();
            }
        }
    }
}
