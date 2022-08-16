using System.Text;
using Duck.Content;
using Duck.Ui.Assets;
using Duck.Ui.RmlUi;
using Duck.Ui.Systems;

namespace Duck.Ui.Content.ContentLoader;

public class UserInterfaceLoader : IAssetLoader
{
    private readonly UiModule _uiModule;

    public UserInterfaceLoader(UiModule uiModule)
    {
        _uiModule = uiModule;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is UserInterface && context is UserInterfaceLoadContext;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not UserInterface shaderAsset || context is not UserInterfaceLoadContext loadContext) {
            throw new Exception("FIXME: errors");
        }

        var rmlContext = loadContext.RmlContext.Context;
        var document = rmlContext.LoadDocumentFromMemory(Encoding.UTF8.GetString(source));

        if (document == null) {
            throw new Exception("TODO: errors");
        }

        document.Show();

        return new RmlUserInterface(loadContext.RmlContext, document);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        // TODO: unload rml user interface
    }
}
