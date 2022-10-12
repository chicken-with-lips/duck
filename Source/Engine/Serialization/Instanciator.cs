using System.Reflection;
using Duck.Serialization.Exception;

namespace Duck.Serialization;

public class Instanciator
{
    private static List<IInstanciator> _instanciators = new();

    public static void Init()
    {
        var entry = Assembly.GetEntryAssembly();

        if (entry == null) {
            return;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies) {
            if (!assembly.GetName().FullName.StartsWith("Duck.")) {
                continue;
            }

            var types = assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.GetInterface(nameof(IInstanciator)) != null);

            foreach (var type in types) {
                _instanciators.Add((IInstanciator)Activator.CreateInstance(type));
            }
        }
    }

    public static void Clear()
    {
        _instanciators.Clear();
    }

    public static T Create<T>(string typeName, IDeserializer deserializer, IDeserializationContext context)
    {
        foreach (var instanciator in _instanciators) {
            if (instanciator.CanInstanciate(typeName)) {
                return (T)instanciator.Instanciate(typeName, deserializer, context);
            }
        }

        throw new SerializationException("Could not find an instanciator for this type: " + typeName);
    }

    public static T Create<T,T2>(string typeName, IDeserializer deserializer, IDeserializationContext context)
        where T2 : struct
    {
        foreach (var instanciator in _instanciators) {
            if (instanciator.CanInstanciate(typeName)) {
                return (T)instanciator.Instanciate<T2>(typeName, deserializer, context);
            }
        }

        throw new SerializationException("Could not find an instanciator for this type: " + typeName);
    }
}

public class BuiltinInstanciator : IInstanciator
{
    public bool CanInstanciate(string typeName)
    {
        return false;
    }

    public object Instanciate(string typeName, IDeserializer deserializer, IDeserializationContext context)
    {
        throw new NotImplementedException();
    }

    public object Instanciate<T>(string typeName, IDeserializer deserializer, IDeserializationContext context)
        where T : struct
    {
        throw new NotImplementedException();
    }
}
