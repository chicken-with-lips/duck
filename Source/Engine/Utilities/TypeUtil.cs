using System.Reflection;

namespace Duck.Utilities;

public static class TypeUtil
{
    public static Type? GetType(string typeName)
    {
        return GetType(typeName, AppDomain.CurrentDomain.GetAssemblies());
    }

    public static Type? GetType(string typeName, IList<Assembly> assemblies)
    {
        return assemblies
            .FirstOrDefault(a => a.GetType(typeName, throwOnError: false) != null)
            ?.GetType(typeName);
    }
}
