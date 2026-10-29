using System.Collections.Generic;
using System.Linq;

namespace Next.Services.Audio;

public class NullAudioService : IAudioService
{
    private readonly Logger? _logger;
    private int _mockVolume = 50;
    private bool _isMuted = false;
    private readonly Dictionary<string, (string Name, int Volume, bool IsMuted)> _mockApps = new()
    {
        ["mock-player"] = ("Mock Player", 70, false),
        ["mock-browser"] = ("Mock Browser", 40, false)
    };

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

    public IReadOnlyList<AppAudioSession> GetAppSessions() =>
        _mockApps.Select(pair => new AppAudioSession(pair.Key, pair.Value.Name, pair.Value.Volume, pair.Value.IsMuted)).ToList();

    public void SetAppVolume(string id, int volume)
    {
        if (!_mockApps.TryGetValue(id, out var app)) return;
        _mockApps[id] = (app.Name, volume, app.IsMuted);
    }

    public void SetAppMuted(string id, bool isMuted)
    {
        if (!_mockApps.TryGetValue(id, out var app)) return;
        _mockApps[id] = (app.Name, app.Volume, isMuted);
    }
}
