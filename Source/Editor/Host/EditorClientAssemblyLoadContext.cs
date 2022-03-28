using System;
using System.IO;
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

        if (assemblyName.FullName.StartsWith("Duck.")) {
            return null;
        }

        Console.WriteLine("EditorClientAssemblyLoadContext.Load: " + assemblyName.FullName);

        // return null;
        // string file = "/home/jolly_samurai/Projects/chicken-with-lips/infectic/Code/bin/Debug/net6.0/" + assemblyName.Name + ".dll";
        string file = "/home/jolly_samurai/Projects/chicken-with-lips/Duck/Build/Debug/net6.0/" + assemblyName.Name + ".dll";

        if (File.Exists(file)) {
            return this.LoadFromAssemblyPath(file);
            // return Assembly.LoadFile(file);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        Console.WriteLine("EditorClientAssemblyLoadContext.LoadUnmanagedDll: " + unmanagedDllName);

        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}
