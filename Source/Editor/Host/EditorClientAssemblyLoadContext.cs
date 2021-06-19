using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Editor.Host;

class EditorClientAssemblyLoadContext : AssemblyLoadContext
{
    public EditorClientAssemblyLoadContext(string? name, bool isCollectible = false) : base(name, isCollectible)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Console.WriteLine("ClientAssemblyLoadContext: " + assemblyName.FullName);

        return null;
    }
}
