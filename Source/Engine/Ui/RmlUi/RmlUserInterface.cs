using ChickenWithLips.RmlUi;
using Duck.Content;
using Duck.Ui.Assets;

namespace Duck.Ui.RmlUi;

public class RmlUserInterface : IPlatformAsset<UserInterface>
{
    #region Members

    private readonly RmlContext _context;
    private readonly ElementDocument _document;

    #endregion

    #region Methods

    internal RmlUserInterface(RmlContext context, ElementDocument document)
    {
        _context = context;
        _document = document;
    }

    #endregion
}
