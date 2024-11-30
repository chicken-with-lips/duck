using Duck.Content;
using Duck.Serialization;

namespace Duck.Audio.Components;

[DuckSerializable]
public partial struct SoundComponent
{
    public AssetReference<SoundClip> Sound;
    public bool IsPlaying;
}
