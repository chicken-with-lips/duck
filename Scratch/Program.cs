// See https://aka.ms/new-console-template for more information

using ADyn.Shapes;
using Arch.Core;
using Duck.Physics.Components;
using Duck.Serialization;
using Duck.Serialization.Arch;
using Game.Components;
using Silk.NET.Maths;

Serializer.Init();

World world = World.Create();
var entity = world.Create<TestComponent, Test3Component>();
ref var testStruct = ref world.Get<TestComponent>(entity);
testStruct.X = 1.2f;
ref var testStruct2 = ref world.Get<Test3Component>(entity);
testStruct2.X = 2.2f;


var entity2 = world.Create<TestComponent>();
ref var testStruct3 = ref world.Get<TestComponent>(entity2);
testStruct3.X = 10;

var entity3 = world.Create<BoxRigidBodyBuilder>();
ref var rigidStruct = ref world.Get<BoxRigidBodyBuilder>(entity3);
rigidStruct.Definition.Gravity = new Vector3D<float>(1, 1, 1);
rigidStruct.Definition.Shape = new BoxShape() {
    HalfExtents = new Vector3D<float>(0.5f, 0.5f, 0.5f),
};
rigidStruct.Definition.Material = new() {
    Damping = 1f,
    Friction = 1.5f,
    Id = 9,
    Restitution = 0.9f,
    RollFriction = 0.8f,
    Stiffness = 0.7f,
};

var entityRef = world.Reference(entity);
var entityRef3 = world.Reference(entity3);

SerializationContext context = new SerializationContext(false);
GraphSerializer graphSerializer = new GraphSerializer(context);

var container = graphSerializer.Close();

var archSerializer = new ArchBinarySerializer();
var writeWorldBytes = archSerializer.Serialize(world);

File.WriteAllBytes("/home/jolly_samurai/test", writeWorldBytes);

var readWorldBytes = File.ReadAllBytes("/home/jolly_samurai/test");
// var readWorldBytes = File.ReadAllBytes(@"D:\Projects\chicken-with-lips\Duck\Build\Test");

var newWorld = archSerializer.Deserialize(readWorldBytes);

Console.WriteLine(newWorld.Get<TestComponent>(entityRef).X);
Console.WriteLine(newWorld.Get<Test3Component>(entityRef).X);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Gravity);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Shape.Value.HalfExtents);
// Console.WriteLine(newWorld.Get<Test3Component>(entityRef).X);

Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.Damping);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.Friction);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.Id);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.Restitution);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.RollFriction);
Console.WriteLine(newWorld.Get<BoxRigidBodyBuilder>(entityRef3).Definition.Material.Value.Stiffness);

Console.WriteLine(testStruct3.X);
   