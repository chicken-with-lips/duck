using System.CommandLine;
using Duck.RenderSystem.Vulkan;

namespace Duck.GameLauncher;

internal static class Program
{
    private static void Main(string[] args)
    {
        var gameOption = new Option<string>(
            "--game",
            "The game to load."
        );

        var command = new RootCommand("Duck") {
            TreatUnmatchedTokensAsErrors = true,
        };
        command.AddOption(gameOption);

        command.SetHandler(
            (gamePath) =>
            {
                var app = new Application(app => new VulkanPlatform(app.CreateLogger("Platform")));
                app.AddModule(
                    new VulkanRenderModule(
                        (VulkanPlatform)app.Platform,
                        app.CreateLogger("Vulkan")
                    )
                );
                app.AddModule(new GameModule(gamePath, app));

                if (app.Initialize()) {
                    app.Run();
                }
            },
            gameOption
        );

        command.Invoke(args);
    }
}