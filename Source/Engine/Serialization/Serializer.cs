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
            var types = assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.GetInterface(nameof(ISerializationFactory)) != null);

            foreach (var type in types) {
                Factories.Add((ISerializationFactory)Activator.CreateInstance(type));
            }
        }
    }

    public static void Clear()
    {
        Factories.Clear();
    }

    public static void Serialize(object value, IGraphSerializer serializer, ISerializationContext context)
    {
        var type = value.GetType();
        var typeName = type.GetFormattedFullName(true);

        if (type.IsGenericType) {
            // var x = 1;
            // typeName = type.Namespace + ".RigidBodyBuilder<TShapeType>";
        }

        foreach (var factory in Factories) {
            if (!factory.Supports(typeName)) {
                continue;
            }

            factory.Serialize(value, serializer, context);
            return;
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + typeName);
    }

    public static object Deserialize(string typeName, IDeserializer deserializer, IDeserializationContext context)
    {
        var type = Type.GetType(typeName);

        if (type != null && type.IsGenericType) {
            var x = 1;
            // typeName = type.Namespace + ".RigidBodyBuilder<TShapeType>";
        }

        foreach (var factory in Factories) {
            if (factory.Supports(typeName)) {
                return factory.Deserialize(typeName, deserializer, context);
            }
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + typeName);
    }

    public static T Deserialize<T>(IDeserializer deserializer, IDeserializationContext context)
    {
        return (T)Deserialize(typeof(T).FullName, deserializer, context);
    }

    /*public static T Deserialize<T>(IDeserializer deserializer, IDeserializationContext context)
    {
        var typeName = typeof(T).FullName;

        foreach (var instanciator in Factories) {
            if (instanciator.Supports(typeName)) {
                return (T)instanciator.Deserialize(typeName, deserializer, context);
            }
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + typeName);
    }

    public static T Deserialize<T, T2>(string typeName, IDeserializer deserializer, IDeserializationContext context)
        where T2 : struct
    {
        foreach (var instanciator in Factories) {
            if (instanciator.Supports(typeName)) {
                return (T)instanciator.Deserialize<T2>(typeName, deserializer, context);
            }
        }

        throw new SerializationException("Could not find a serialization factory for this type: " + typeName);
    }*/

    private static string GetFormattedFullName(this Type type, bool useGenericArgumentsInsteadOfParameters)
    {
        // https://stackoverflow.com/a/66604069

        if (type.IsGenericType) {
            string genericArguments = (useGenericArgumentsInsteadOfParameters ? type.GetGenericArguments() : type.GenericTypeArguments)
                .Select(x => x.Name)
                .Aggregate((x1, x2) => $"{x1}, {x2}");

            return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                   + $"<{genericArguments}>";
        }

        return type.FullName;
    }
}
