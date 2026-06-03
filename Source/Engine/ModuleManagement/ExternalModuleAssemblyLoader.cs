using System.Reflection;

namespace Duck.ModuleManagement;

public static class ExternalModuleAssemblyLoader
{
    public static ExternalModuleHandle Load(string dllPath)
    {
        var fullPath = Path.GetFullPath(dllPath);
        var context = new ExternalModuleAssemblyLoadContext(fullPath);

        using (context.EnterContextualReflection()) {
            using var stream = File.OpenRead(fullPath);

            var assembly = context.LoadFromStream(stream);
            var systemType = assembly
                .GetTypes()
                .First(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract);

            var instance = (IModule)Activator.CreateInstance(systemType)!;

            return new ExternalModuleHandle(context, assembly, instance);
        }
    }
}

public sealed class ExternalModuleHandle : IDisposable
{
    public ExternalModuleAssemblyLoadContext LoadContext { get; }
    public Assembly Assembly { get; private set; }
    public IModule Module { get; }

    public ExternalModuleHandle(ExternalModuleAssemblyLoadContext context, Assembly assembly, IModule module)
    {
        LoadContext = context;
        Assembly = assembly;
        Module = module;
    }

    public void Dispose()
    {
        if (Module is IDisposable disposable) {
            disposable.Dispose();
        }

        LoadContext.Unload();
    }
}
