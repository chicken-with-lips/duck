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
        // string file = "/home/jolly_samurai/Projects/chicken-with-lips/infectic/Code/bin/Debug/net7.0/" + assemblyName.Name + ".dll";
        string file = "/media/jolly_samurai/Data/Projects/chicken-with-lips/Duck/Build/Debug/net7.0/" + assemblyName.Name + ".dll";

        if (File.Exists(file)) {
            return LoadFromAssemblyPath(file);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        Console.WriteLine("EditorClientAssemblyLoadContext.LoadUnmanagedDll: " + unmanagedDllName);

        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}
