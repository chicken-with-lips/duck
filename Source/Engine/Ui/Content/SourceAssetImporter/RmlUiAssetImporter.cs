using Duck.Content;
using Duck.Content.SourceAssetImporter;
using Duck.Ui.Assets;

namespace Duck.Ui.Content.SourceAssetImporter;

public class RmlUiAssetImporter : SourceAssetImporterBase<UserInterface>
{
    public RmlUiAssetImporter()
    {
    }

    public override bool CanImport(string file)
    {
        return Path.GetExtension(file).ToLower() == ".rml";
    }

    public override UserInterface Import(string file)
    {
        return new UserInterface(
            new AssetImportData(new Uri("file://" + file))
        );
    }
}
