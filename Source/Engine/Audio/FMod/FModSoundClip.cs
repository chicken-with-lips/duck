using Duck.Content;
using FmodAudio;
using WeakEvent;

namespace Duck.Audio.FMod;

public class FModSoundClip : PlatformAssetBase<SoundClip>
{
    public Sound Sound { get; internal set; }

    public FModSoundClip(Sound sound)
    {
        Sound = sound;
    }
}
