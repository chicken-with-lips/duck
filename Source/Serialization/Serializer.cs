using System.Reflection;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public static class Serializer
{
    private static readonly List<ISerializationFactory> Factories = new();

    public static void Init()
    {
        // FIXME: Replace with generator.
        var entry = Assembly.GetEntryAssembly();

        if (entry == null) {
            return;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies) {
            var types = assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && type.GetInterface(nameof(ISerializationFactory)) != null);

            foreach (var type in types) {
                Console.WriteLine(assembly.FullName);
                var typeName = type.FullName ?? throw new ArgumentException("Type does not have a full name");
                var instance = assembly.CreateInstance(typeName)
                               ?? throw new InvalidOperationException();

                Factories.Add((ISerializationFactory)instance);
            }
        }
    }

    public static void Clear()
    {
        Factories.Clear();
    }

    public static void Serialize(object value, GraphWriter writer)
    {
        var type = value.GetType();
        var typeName = type.GetFormattedFullName(true);

        foreach (var factory in Factories) {
            if (!factory.Supports(typeName)) {
                continue;
            }

            factory.Serialize(value, writer);
            return;
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + typeName);
    }

    public static T Deserialize<T>(GraphReader reader)
    {
        return (T)Deserialize(typeof(T).FullName!, reader);
    }

    public static object Deserialize(string typeName, GraphReader reader)
    {
        return GetFactory(typeName)
            .Deserialize(typeName, reader);
    }

    public static ISerializationFactory GetFactory<T>()
    {
        return GetFactory(typeof(T).FullName!);
    }

    public static ISerializationFactory GetFactory(string serializableTypeName)
    {
        foreach (var factory in Factories) {
            if (factory.Supports(serializableTypeName)) {
                return factory;
            }
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + serializableTypeName
        );
    }

    private static string GetFormattedFullName(this Type type, bool useGenericArgumentsInsteadOfParameters)
    {
        // https://stackoverflow.com/a/66604069

        if (type.IsGenericType) {
            var genericArguments = (useGenericArgumentsInsteadOfParameters
                    ? type.GetGenericArguments()
                    : type.GenericTypeArguments)
                .Select(x => x.Name)
                .Aggregate((x1, x2) => $"{x1}, {x2}");

            return $"{type.Name.Substring(0, type.Name.IndexOf('`'))}" + $"<{genericArguments}>";
        }

        return type.FullName!;
    }
}