using Duck.Content;

namespace Duck.Audio;

public interface IAudioModule : IModule
{
    public void PlaySound(AssetReference<SoundClip> soundClip);
}
