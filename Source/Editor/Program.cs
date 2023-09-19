using System.CommandLine;
using System.Runtime.CompilerServices;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL;

namespace Editor;

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static unsafe void Main(string[] args)
    {
        var projectOption = new Option<string>(
            name: "--project",
            description: "The project to load.");

        var rootCommand = new RootCommand("Duck Editor") {
            TreatUnmatchedTokensAsErrors = true
        };
        rootCommand.AddOption(projectOption);

        rootCommand.SetHandler((projectDirectory) => {
            var app = new EditorApp(
                new StandardPlatform(),
                new OpenGLRenderSystem(),
                projectDirectory
            );

            if (app.Initialize()) {
                app.Run();
            }
        }, projectOption);

        rootCommand.Invoke(args);
    }
}
