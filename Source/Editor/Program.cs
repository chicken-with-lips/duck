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
