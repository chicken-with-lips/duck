namespace Duck.Content;

public interface IAssetLoader
{
    bool CanLoad(IAsset asset);
    IPlatformAsset Load(IAsset asset);
    void Unload(IAsset asset, IPlatformAsset platformAsset);
}