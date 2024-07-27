using Duck.Audio.Content.Loaders;
using Duck.Audio.FMod;
using Duck.Content;
using Duck.Logging;
using Duck.Platform;
using FmodAudio;

namespace Duck.Audio;

public class AudioModule : IAudioModule, IInitializableModule
{
    private readonly IContentModule _contentModule;

    #region Members

    private readonly ILogger _logger;
    private FmodSystem _system;

    #endregion

    #region Methods

    public AudioModule(ILogModule logModule, IContentModule contentModule)
    {
        _contentModule = contentModule;

        _logger = logModule.CreateLogger("Audio");
        _logger.LogInformation("Created audio module.");
    }

    public bool Init()
    {
        _logger.LogInformation("Initializing audio module...");
        _logger.LogInformation("Creating FMod audio system");

        _system = Fmod.CreateSystem();
        _system.Init(2);
        
        _logger.LogInformation("2 audio channels");

        _contentModule
            .RegisterAssetLoader<SoundClip, FModSoundClip>(new SoundClipLoader(_system));

        return true;
    }

    public void PlaySound(AssetReference<SoundClip> soundClip)
    {
        var sound = _contentModule.LoadImmediate(soundClip);

        _system.PlaySound(((FModSoundClip)sound).Sound);
    }

    #endregion
}
