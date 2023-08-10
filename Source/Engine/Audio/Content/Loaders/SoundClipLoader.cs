using System.Diagnostics;
using Duck.Audio.FMod;
using Duck.Content;
using FmodAudio;

namespace Duck.Audio.Content.Loaders;

internal class SoundClipLoader : IAssetLoader
{
    private readonly FmodSystem _system;

    public SoundClipLoader(FmodSystem system)
    {
        _system = system;
    }

    public bool CanLoad(IAsset asset, IAssetLoadContext context)
    {
        return asset is SoundClip;
    }

    public IPlatformAsset Load(IAsset asset, IAssetLoadContext context, IPlatformAsset? loadInto, ReadOnlySpan<byte> source)
    {
        if (!CanLoad(asset, context) || asset is not SoundClip textureAsset) {
            throw new Exception("FIXME: errors");
        }

        var sound = _system.CreateSound(source, Mode.Default  | Mode.OpenMemory, new CreateSoundInfo {
            Length = (uint)source.Length,
        });

        if (loadInto != null) {
            ((FModSoundClip)loadInto).Sound.Dispose();

            Debug.Assert(loadInto is FModSoundClip);

            ((FModSoundClip)loadInto).Sound = sound;
        }

        return new FModSoundClip(sound);
    }

    public void Unload(IAsset asset, IPlatformAsset platformAsset)
    {
        if (asset.IsLoaded && platformAsset is FModSoundClip soundClip) {
            soundClip.Sound.Dispose();
        }
    }
}
