using System.Reflection;

namespace Duck.Platform.ModuleManagement;

public delegate void ExternalModuleChanged(ExternalModuleManager manager);

public class ExternalModuleManager : IDisposable
{
    #region Properties

    public event ExternalModuleChanged? Changed;

    public string AssemblyPath { get; }

    public bool IsPendingUnloadComplete
    {
        get => !_isUnloading;
    }

    public ExternalModuleHandle? Handle
    {
        get => _current;
    }

    #endregion

    #region Members

    private FileSystemWatcher? _watcher;
    private long _assemblyLastWriteTimeTicks;

    private ExternalModuleHandle? _current;

    private bool _isUnloading = false;
    private WeakReference<ExternalModuleAssemblyLoadContext>? _unloadingContext;

    #endregion

    public ExternalModuleManager(string dllPath)
    {
        AssemblyPath = Path.GetFullPath(dllPath);
    }

    public void Initialize()
    {
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(AssemblyPath)!) {
            Filter = Path.GetFileName(AssemblyPath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
        };

        _watcher.Changed += (_, __) => RaiseChangedEvent();
        _watcher.EnableRaisingEvents = true;

        Reload();
    }

    private void RaiseChangedEvent()
    {
        var now = DateTime.UtcNow.Ticks;
        var last = Interlocked.Exchange(ref _assemblyLastWriteTimeTicks, now);

        if (now - last
            > TimeSpan.FromMilliseconds(100)
                .Ticks) {
            Changed?.Invoke(this);
        }
    }

    public void RequestUnload()
    {
        if (_current == null || _isUnloading) {
            return;
        }

        _unloadingContext = new WeakReference<ExternalModuleAssemblyLoadContext>(_current.LoadContext);

        _current.Dispose();
        _current = null;

        _isUnloading = true;
    }

    public void PumpUnload()
    {
        if (!_isUnloading) {
            return;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForFullGCComplete();

        if (_unloadingContext?.TryGetTarget(out _) == true) {
            return;
        }

        _isUnloading = false;
        _unloadingContext = null;
    }

    public Tuple<Assembly?, Assembly> Reload()
    {
        var currentAssembly = _current?.Assembly;

        RequestUnload();

        // tiny retry loop in case the file is locked by build/copy
        for (var i = 0; i < 10; i++) {
            try {
                _current = ExternalModuleAssemblyLoader.Load(AssemblyPath);

                Console.WriteLine($"[HotReload] Loaded system from {AssemblyPath}");
                break;
            } catch (IOException) {
                Thread.Sleep(100);
            } catch (Exception ex) {
                Console.WriteLine($"[HotReload] Failed to load: {ex}");
                Thread.Sleep(500);
            }
        }

        if (_current == null) {
            throw new InvalidOperationException("Failed to load module");
        }

        return new Tuple<Assembly?, Assembly>(currentAssembly, _current!.Assembly);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _watcher = null;

        _current?.Dispose();
        _current = null;
    }
}