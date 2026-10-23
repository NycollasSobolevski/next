namespace Next.Services.Audio;

public class NullAudioService : IAudioService
{
    private readonly Logger? _logger;
    private int _mockVolume = 50;
    private bool _isMuted = false;

    public NullAudioService(Logger? logger = null)
    {
        _logger = logger;
        _logger?.WriteLine("[NullAudioService] Using dummy audio service (no OS audio controller available).");
    }

    public int GetMasterVolume() => _mockVolume;

    public void SetMasterVolume(int volume)
    {
        _mockVolume = volume;
        _logger?.WriteLine($"[NullAudioService] Master volume set to {volume}%");
    }

    public bool IsMuted() => _isMuted;

    public void SetMuted(bool isMuted)
    {
        _isMuted = isMuted;
        _logger?.WriteLine($"[NullAudioService] Mute set to {isMuted}");
    }
}
