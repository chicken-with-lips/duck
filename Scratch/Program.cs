// See https://aka.ms/new-console-template for more information

using System;
using Arch.Core;
using Duck.Serialization;

var t = new[] {
    new TestStruct {
        X = 5,
    },
    new TestStruct {
        X = 4,
    }
};

var world = World.Create();
world.Create<TestStruct>(new TestStruct() {
    X = 5,
    Y = 2,
});
world.Create<TestStruct>(new TestStruct() {
    X = 15,
    Y = 12,
});

var context = new SerializationContext(false);
var deserializationContext = new DeserializationContext(context);
 var graphWriter = new GraphWriter(context);
 var writer = new Writer();

Serializer.Init();

 // graphWriter.Write("types", t);
// writer.Write(t, context);
// Serializer.Serialize(t, graphWriter, context);

// graphWriter.Write("World",  world);
 Serializer.Serialize(world, graphWriter);

// var writer = new Writer();
// writer.Write<TestStruct>(t);

var container = graphWriter.Close();

 var graphReader = new GraphReader(container.Data, container.Index, context);
var reader = new Reader(container.Data, context);
// reader.ReadObjectList<TestStruct[], TestStruct>(container);

// t[1].X = 100;

world = Serializer.Deserialize<World>(graphReader);

// reader.ReadObjectList<TestStruct[], TestStruct>(ref t);
// var z = 1;

// var x = Serializer.Deserialize<TestStruct>(reader, deserializationContext);

// Console.WriteLine(x.X);
// Console.WriteLine(t[1].X);

var queryDesc = new QueryDescription().WithAll<TestStruct>();
world.Query(queryDesc, entity => Console.WriteLine(entity.Id + " -> " + world.Get<TestStruct>(entity).X));

[DuckSerializable]
public struct TestStruct
{
    public float X;
    public float Y;
}
