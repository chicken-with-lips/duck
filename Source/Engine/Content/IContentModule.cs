using Duck.Platform.ModuleManagement;

namespace Duck.Content;

public interface IContentModule : IModule
{
    #region Properties

    IAssetDatabase Database { get; }
    bool ShouldReloadChangedContent { get; set; }

    #endregion

    #region Methods

    void AddContentDirectory(string prefix, string directory);

    IContentModule RegisterAssetLoader<TAsset, TPlatformAsset>(IAssetLoader loader)
        where TAsset : class, IAsset
        where TPlatformAsset : class, IPlatformAsset;

    IAssetLoader? FindAssetLoader<TAsset>(TAsset asset)
        where TAsset : class, IAsset;

    #endregion
}