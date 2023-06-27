using ChickenWithLips.ArteryFont;
using Duck.Content;
using Duck.Renderer.Textures;
using Duck.Ui;
using Duck.Ui.Artery;

namespace Duck.UI.Content;

internal class FontLoader : IAssetLoader
{
    private readonly IContentModule _contentModule;

    public FontLoader(IContentModule contentModule)
    {
        _contentModule = contentModule;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is Font;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not Font fontAsset) {
            throw new Exception("FIXME: errors");
        }

        var font = FontSerializer<float>.Default.Deserialize(source);
        var textures = new IPlatformAsset<Texture2D>[font.Images.Length];

        for (var i = 0; i < font.Images.Length; i++) {
            var imageAssetName = $"{asset.Id.ToString()}_{i}";
            var imageAsset = _contentModule.Database.Register(
                new Texture2D(new AssetImportData(new Uri("memory://" + imageAssetName)), (int)font.Images[i].Width, (int)font.Images[i].Height)
            );
            textures[i] = (IPlatformAsset<Texture2D>)_contentModule.LoadImmediate(imageAsset.MakeSharedReference(), EmptyAssetLoadContext.Default, font.Images[i].Data);
        }

        return new ArteryFont(font, textures);

    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is Font fontAsset) {
            throw new Exception("TODO");
        }
    }
}
