using System.Diagnostics;
using System.Text;
using Duck.Content;
using Duck.Logging;
using Duck.Ui.Assets;
using Duck.Ui.RmlUi;
using Duck.Ui.Systems;
using Context = ChickenWithLips.RmlUi.Context;

namespace Duck.Ui.Content.ContentLoader;

public class UserInterfaceLoader : IAssetLoader
{
    private readonly UiModule _uiModule;
    private readonly ILogger _logger;

    public UserInterfaceLoader(UiModule uiModule, ILogger logger)
    {
        _uiModule = uiModule;
        _logger = logger;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is UserInterface;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not UserInterface shaderAsset) {
            throw new Exception("FIXME: errors");
        }

        Context? rmlContext = null;
        UserInterfaceLoadContext loadContext = new UserInterfaceLoadContext();

        if (context is UserInterfaceLoadContext) {
            loadContext = (UserInterfaceLoadContext)context;
            rmlContext = loadContext.RmlContext.Context;
        } else if (loadInto is RmlUserInterface rmlUserInterface) {
            rmlContext = rmlUserInterface.Context.Context;
        } else {
            throw new Exception("TODO: errors");
        }

        var document = rmlContext.LoadDocumentFromMemory(Encoding.UTF8.GetString(source));

        if (document == null) {
            _logger.LogError("Error loading document.");

            document = rmlContext.LoadDocumentFromMemory("<html></html>");
        }

        if (document == null) {
            throw new Exception("TODO: errors");
        }

        document.Show();

        if (loadInto != null && loadInto is RmlUserInterface existingUserInterface) {
            existingUserInterface.Document.Close();
            existingUserInterface.Document = document;

            return existingUserInterface;
        }

        return new RmlUserInterface(loadContext.RmlContext, document);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        // TODO: unload rml user interface
        throw new Exception("TODO: unload");
    }
}

internal struct UserInterfaceLoadContext : IAssetLoadContext
{
    public RmlContext RmlContext;
}
