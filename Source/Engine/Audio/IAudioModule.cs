using Duck.Content;
using Duck.Platform;

namespace Duck.Audio;

public interface IAudioModule : IModule
{
    public void PlaySound(AssetReference<SoundClip> soundClip);
}
