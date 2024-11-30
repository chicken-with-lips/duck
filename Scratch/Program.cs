// See https://aka.ms/new-console-template for more information

using Arch.Core;
using Duck.Serialization;
using Game.Components;
using Scratch;

World world = World.Create();
var entity = world.Create<TestComponent, Test3Component>();
ref var testStruct = ref world.Get<TestComponent>(entity);
testStruct.X = 1.2f;
ref var testStruct2 = ref world.Get<Test3Component>(entity);
testStruct2.X = 2.2f;


var entity2 = world.Create<TestComponent>();
ref var testStruct3 = ref world.Get<TestComponent>(entity2);
testStruct3.X = 10;

var entityRef = world.Reference(entity);

SerializationContext context = new SerializationContext(false);
// StandardSerializer serializer = new StandardSerializer();
GraphSerializer graphSerializer = new GraphSerializer(context);
//
// foreach (var worldArchetype in world.Archetypes) {
//     foreach (var worldArchetypeChunk in worldArchetype.Chunks) {
//         
//         foreach (var worldEntity in worldArchetypeChunk) {
//             
//         }
//     }
// }

testStruct3.Serialize(graphSerializer, context);
var container = graphSerializer.Close();
// serializer.Write(testStruct3);

// serializer.Write();

// serializer.Write(world);

var archSerializer = new ArchBinarySerializer();
var worldBytes = archSerializer.Serialize(world);

// File.WriteAllBytes("/home/jolly_samurai/test", worldBytes);

// var worldBytes = File.ReadAllBytes("/home/jolly_samurai/test");
// var worldBytes = File.ReadAllBytes(@"D:\Projects\chicken-with-lips\Duck\Build\Test");

var deserializer = new Deserializer(container.Data, container.Index, context);

testStruct3 = new TestComponent(deserializer, new DeserializationContext(null, context));


var newWorld = archSerializer.Deserialize(worldBytes);

// Console.WriteLine(newWorld.Get<TestComponent>(entityRef).X);
// Console.WriteLine(newWorld.Get<TestComponent>(entityRef).Y);
Console.WriteLine(newWorld.Get<Test3Component>(entityRef).X);

Console.WriteLine(testStruct3.X);