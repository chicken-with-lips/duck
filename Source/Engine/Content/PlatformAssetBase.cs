using WeakEvent;

namespace Duck.Content;

public class PlatformAssetBase<T> : IPlatformAsset<T>
    where T : class, IAsset
{
    private readonly WeakEventSource<ReloadEvent> _reloadedEventSource = new();

    public WeakEventSource<ReloadEvent> Reloaded { get; } = new();
}
