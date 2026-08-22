using System.Collections.Concurrent;
using System.Diagnostics;
using Duck.Platform.Logging;
using Duck.Platform.ModuleManagement;

namespace Duck.Content;

public class ContentModule : IContentModule, IInitializableModule
{
    #region Properties

    public bool ShouldReloadChangedContent {
        get;

        set {
            AssertModuleWasInitialized();

            if (value) {
                StartWatchingForContentChanges();
            } else {
                StopWatchingForContentChanges();
            }
        }
    } = false;

    public IAssetDatabase Database {
        get {
            AssertModuleWasInitialized();

            return _database!;
        }
    }

    #endregion

    #region Members

    private readonly Logger _logger;
    private IAssetDatabase? _database;
    private bool _isInitialized;

    private readonly HashSet<IAssetLoader> _assetLoaders = new();
    private readonly HashSet<ContentDirectory> _contentDirectories = new();

    private readonly ConcurrentDictionary<Type, IPlatformAssetCollection> _assetCaches = new();

    #endregion

    #region Methods

    public ContentModule(Logger logger)
    {
        _logger = logger;
    }

    public void Initialize(IApplication app, IInitializationContext context)
    {
        if (context.WasHotReloaded) {
            return;
        }

        _logger.LogInformation("Initializing content module");
        _database = new AssetDatabase(_logger);
        _isInitialized = true;

        AddContentDirectory("duck", Environment.CurrentDirectory);
    }

    public void AddContentDirectory(string prefix, string path)
    {
        AssertModuleWasInitialized();

        if (null != FindContentDirectoryByPath(path)) {
            _logger.LogError(
                $"Cannot add content directory because it already exists: {path}"
            );

            return;
        }

        if (null != FindContentDirectoryByPrefix(prefix)) {
            _logger.LogError(
                $"Cannot add content directory because it already exists: {path}"
            );

            return;
        }

        _logger.LogInformation(
            $"Adding content directory \"{path}\" with prefix \"{prefix}\""
        );

        _contentDirectories.Add(
            new ContentDirectory(
                prefix,
                path
            )
        );

        if (ShouldReloadChangedContent) {
            StartWatchingForContentChanges();
        }
    }


    public IContentModule RegisterAssetLoader<TAsset, TPlatformAsset>(IAssetLoader loader)
        where TAsset : class, IAsset
        where TPlatformAsset : class, IPlatformAsset
    {
        _assetLoaders.Add(loader);
        _assetCaches.TryAdd(typeof(TAsset), new PlatformAssetCollection<TAsset, TPlatformAsset>());

        return this;
    }

    public IAssetLoader? FindAssetLoader<TAsset>(TAsset asset)
        where TAsset : class, IAsset
    {
        foreach (var assetLoader in _assetLoaders) {
            if (assetLoader.CanLoad(asset)) {
                return assetLoader;
            }
        }

        return null;
    }

    private ContentDirectory? FindContentDirectoryByPath(string path)
    {
        foreach (var contentDirectory in _contentDirectories) {
            if (contentDirectory.Path == path) {
                return contentDirectory;
            }
        }

        return null;
    }

    private ContentDirectory? FindContentDirectoryByPrefix(string prefix)
    {
        foreach (var contentDirectory in _contentDirectories) {
            if (contentDirectory.Prefix == prefix) {
                return contentDirectory;
            }
        }

        return null;
    }

    private void StartWatchingForContentChanges()
    {
        _contentDirectories.ForEach(dir => dir.StartWatching(OnContentChanged));
    }


    private void StopWatchingForContentChanges()
    {
        _contentDirectories.ForEach(dir => dir.StopWatching(OnContentChanged));
    }

    private void OnContentChanged(object source, FileSystemEventArgs e)
    {
        _logger.LogWarning("OnContentChanged: not implemented");
    }

    private void AssertModuleWasInitialized()
    {
        if (!_isInitialized) {
            throw new InvalidOperationException("Content module was not initialized");
        }
    }

    #endregion

    private class ContentDirectory
    {
        public string Prefix { get; }
        public string Path { get; }
        public FileSystemWatcher? Watcher { get; private set; }

        public ContentDirectory(string prefix, string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(prefix));
            Debug.Assert(!string.IsNullOrEmpty(path));

            Prefix = prefix;
            Path = path;
        }

        public void StartWatching(FileSystemEventHandler handler)
        {
            if (Watcher != null) {
                return;
            }

            Watcher = new FileSystemWatcher();
            Watcher.Path = Path;
            Watcher.NotifyFilter = NotifyFilters.LastWrite;
            Watcher.IncludeSubdirectories = true;
            Watcher.EnableRaisingEvents = true;
            Watcher.Changed += handler;
        }

        public void StopWatching(FileSystemEventHandler handler)
        {
            if (Watcher == null) {
                return;
            }

            Watcher.Changed -= handler;
            Watcher.Dispose();
            Watcher = null;
        }
    }
}