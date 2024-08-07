using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Audio.Components;
using Duck.Content;

namespace Duck.Audio.Systems;

public partial class PlaySoundSystem : BaseSystem<World, float>
{
    private readonly IAudioModule _audioModule;

    public PlaySoundSystem(World world, IAudioModule audioModule)
        : base(world)
    {
        _audioModule = audioModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, ref SoundComponent component)
    {
        if (component.Sound == AssetReference<SoundClip>.Null || component.IsPlaying) {
            return;
        }

        component.IsPlaying = true;
        _audioModule.PlaySound(component.Sound);
    }
}
