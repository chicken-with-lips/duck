namespace Duck.Content;

public interface IAssetLoader
{
    public bool CanLoad(IAsset asset);
    public IPlatformAsset Load(IAsset asset, ReadOnlySpan<byte> source);
    public void Unload(IAsset asset, IPlatformAsset platformAsset);
}
