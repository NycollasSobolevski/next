using System.Collections.Generic;

namespace Next.Services.Audio;

public interface IAudioService
{
    /// <summary>
    /// Gets the current master volume as an integer percentage (0 to 100).
    /// Returns -1 if volume could not be determined.
    /// </summary>
    int GetMasterVolume();

    /// <summary>
    /// Sets the master volume as an integer percentage (0 to 100).
    /// </summary>
    void SetMasterVolume(int volume);

    /// <summary>
    /// Gets whether master audio output is currently muted.
    /// </summary>
    bool IsMuted();

    /// <summary>
    /// Sets master audio output mute status.
    /// </summary>
    void SetMuted(bool isMuted);

    /// <summary>
    /// Lists applications that currently have an audio session, one entry per application.
    /// </summary>
    IReadOnlyList<AppAudioSession> GetAppSessions();

    /// <summary>
    /// Sets the volume (0 to 100) of every audio session belonging to the application with the given id.
    /// </summary>
    void SetAppVolume(string id, int volume);

    /// <summary>
    /// Sets the mute status of every audio session belonging to the application with the given id.
    /// </summary>
    void SetAppMuted(string id, bool isMuted);
}
