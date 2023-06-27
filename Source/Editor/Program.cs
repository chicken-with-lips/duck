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
            var app = new Editor(
                new StandardPlatform(),
                new OpenGLRenderSystem(),
                projectDirectory
            );

            if (app.Initialize()) {
                app.Run();
            }
        }, projectOption);

        rootCommand.Invoke(args);

        return;

        // int iterationCount = 0;
        // bool foo = false;
        // //
        //
        // var c = new Context(1280, 1024);
        // c.AddElementType<Root>(new RootFactory());
        // c.AddElementType<Window>(new WindowFactory());
        // c.AddElementType<Button>(new ButtonFactory());
        // c.AddElementType<Label>(new LabelFactory());
        // c.AddElementType<Panel>(new PanelFactory());
        // c.AddElementType<HorizontalContainer>(new HorizontalContainerFactory());
        //
        // var r = new RenderList();
        // var rootRenderer = new RootRenderer();
        //
        // while (true) {
        //     c.BeginFrame();
        //
        //     c.New(
        //         c.HorizontalContainer(
        //             HorizontalContainerProps.Default,
        //             c.Panel(
        //                 PanelProps.Default with {
        //                     Position = new Vector2D<int>(500, 100)
        //                 }
        //             )
        //         )
        //     );
        //
        //
        //     foreach (ref var cRoot in c.Roots) {
        //         rootRenderer.Render(ref cRoot, r);
        //     }
        //
        //     foo = !foo;
        //
        //     if (iterationCount++ > 50000000) {
        //         break;
        //     }
        // }

        // while (true) {
        //     Button2Factory.Reset();
        //     Window2Factory.Reset();
        //     Label2Factory.Reset();
        //
        //     var r = new Root2(
        //         Button2Factory.Create(ButtonProps.Default with {
        //             Label = "Button1",
        //         }),
        //         Button2Factory.Create(ButtonProps.Default with {
        //             Label = "Button2",
        //         }),
        //         Button2Factory.Create(ButtonProps.Default with {
        //             Label = "Button3",
        //         }),
        //         CreateWindow(foo),
        //         null
        //     );
        //
        //     // int[] list = new int[100];
        //
        //     r.C0.Value.Renderer.ProcessFragment(r.C0.Value);
        //     r.C1.Value.Renderer.ProcessFragment(r.C1.Value);
        //     r.C2.Value.Renderer.ProcessFragment(r.C2.Value);
        //     r.C3.Value.Renderer.ProcessFragment(r.C3.Value);
        //
        //     iterationCount++;
        //
        //     if (iterationCount > 10000000) {
        //         break;
        //     }
        //
        //     foo = !foo;
        // }

        // Instanciator.Init();

        /*var logSubsystem = new LogSubsystem();
        var eventSubsystem = new EventBus();
        var worldSubsystem = new WorldSubsystem(logSubsystem, eventSubsystem);
        var subsystem = new SceneSubsystem(worldSubsystem);
        subsystem.Create();
        subsystem.Create();
        subsystem.Create();

        var context = new SerializationContext(true);
        var serializer = new GraphSerializer(context);

        var test = new TransformComponent();
        test.Translation = Vector3D<float>.UnitX;

        int hash = test.GetHashCode();
        test = new TransformComponent();
        test.Rotation = Quaternion<float>.CreateFromYawPitchRoll(10, 10, 10);
        hash = test.GetHashCode();

        var world = worldSubsystem.Create();
        var entityPool = world.CreateEntity();
        ref var cmp = ref entityPool.Get<TransformComponent>();
        cmp.Translation = Vector3D<float>.UnitX;
        var entityPool2 = world.CreateEntity();

        // subsystem.Serialize(serializer, context);
        // serializer.Write(subsystem);
        serializer.Write("x", entityPool);
        serializer.Write("y", entityPool2);

        var container = serializer.Close();

        var deContext = new SerializationContext(true);
        var deserializer = new Deserializer(container.Data, container.Index, deContext, null);
        // var test2 = deserializer.ReadObject<SceneSubsystem>((deserializer1) => {
            // return new SceneSubsystem(deserializer1);
        // });

        var test2 = deserializer.ReadObjectReference<Entity>((deserializer1, context1, entry1) => {
            return new Entity(deserializer1, context1);
        }, container.Index[1]);
        var test3 = deserializer.ReadObjectReference<Entity>((deserializer1, context1, entry1) => {
            return new Entity(deserializer1, context1);
        }, container.Index[2]);

        var x = 1;*/
    }
}
