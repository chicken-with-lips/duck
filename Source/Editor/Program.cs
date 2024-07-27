using System;
using System.CommandLine;
using System.Runtime.CompilerServices;
using Duck.Platforms.Standard;
using Duck.RenderSystems.OpenGL;
using Duck.Serialization;
using Silk.NET.Maths;

namespace Editor;

[AutoSerializable]
partial struct Test2 : ISerializable
{
    public int Y;
}

[AutoSerializable]
partial struct Test
{
    public int X;

    public Vector3D<float> Y;

    public Test2 Z;
}

class Program
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    static unsafe void Main(string[] args)
    {
        Instanciator.Init();

        var t = new Test();
        t.X = 100;
        t.Y.Y = 50;
        t.Z.Y = 125;

        var context = new SerializationContext(false);
        var s = new GraphSerializer(context);
        t.Serialize(s, context);
        var container = s.Close();

        var deserializationContext = new DeserializationContext(null, new SerializationContext(false));
        var d = new Deserializer(container.Data, container.Index, deserializationContext);

        var t2 = new Test(d, deserializationContext);
        
        Console.WriteLine(t2.X);
        Console.WriteLine(t2.Y.Y);
        Console.WriteLine(t2.Z.Y);
        return;

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
