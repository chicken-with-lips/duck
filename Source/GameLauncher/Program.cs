using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL;

namespace GameLauncher
{
    class Program
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Main(string[] args)
        {
            var projectOption = new Option<string>(
                name: "--project",
                description: "The project to load.",
                getDefaultValue: () => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
            );

            var rootCommand = new RootCommand("Duck") {
                TreatUnmatchedTokensAsErrors = true
            };
            rootCommand.AddOption(projectOption);

            rootCommand.SetHandler((projectDirectory) => {
                var app = new Game(
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
}
