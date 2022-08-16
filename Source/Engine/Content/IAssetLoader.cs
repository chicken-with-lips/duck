namespace Duck.Content;

public interface IAssetLoader
{
    public bool CanLoad(IAsset asset, IAssetLoadContext context);
    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, ReadOnlySpan<byte> source);
    public void Unload(IAsset asset, IPlatformAsset platformAsset);
}
