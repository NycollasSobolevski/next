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
}
