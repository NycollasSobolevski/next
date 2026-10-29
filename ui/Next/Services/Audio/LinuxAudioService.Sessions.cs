using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Next.Services.Audio;

public partial class LinuxAudioService
{
    private readonly object _appThrottleLock = new();
    private readonly Dictionary<string, int> _pendingAppVolumes = new();
    private bool _appVolumeProcessing;
    private string? _pactlPath;
    private bool _pactlResolved;

    private string? PactlPath
    {
        get
        {
            if (!_pactlResolved)
            {
                _pactlPath = File.Exists("/usr/bin/pactl") ? "/usr/bin/pactl" : FindInPath("pactl");
                _pactlResolved = true;
            }
            return _pactlPath;
        }
    }

    public IReadOnlyList<AppAudioSession> GetAppSessions()
    {
        if (PactlPath == null) return [];

        try
        {
            string json = RunCommand(PactlPath, "-f json list sink-inputs");
            return ParseSinkInputs(json);
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error listing app sessions: {ex.Message}");
            return [];
        }
    }

    public void SetAppVolume(string id, int volume)
    {
        if (PactlPath == null) return;

        lock (_appThrottleLock)
        {
            _pendingAppVolumes[id] = Math.Clamp(volume, 0, 100);
            if (_appVolumeProcessing) return;
            _appVolumeProcessing = true;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                KeyValuePair<string, int>[] pending;
                lock (_appThrottleLock)
                {
                    pending = _pendingAppVolumes.ToArray();
                    _pendingAppVolumes.Clear();
                }

                foreach (var (appId, appVolume) in pending)
                {
                    ApplyAppVolume(appId, appVolume);
                }

                await Task.Delay(25);

                lock (_appThrottleLock)
                {
                    if (_pendingAppVolumes.Count == 0)
                    {
                        _appVolumeProcessing = false;
                        break;
                    }
                }
            }
        });
    }

    public void SetAppMuted(string id, bool isMuted)
    {
        if (PactlPath == null) return;

        try
        {
            foreach (var index in FindSinkInputIndices(id))
            {
                RunCommand(PactlPath!, $"set-sink-input-mute {index} {(isMuted ? "1" : "0")}");
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error setting mute of {id}: {ex.Message}");
        }
    }

    private void ApplyAppVolume(string id, int volume)
    {
        try
        {
            foreach (var index in FindSinkInputIndices(id))
            {
                RunCommand(PactlPath!, $"set-sink-input-volume {index} {volume}%");
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error setting volume of {id}: {ex.Message}");
        }
    }

    private IEnumerable<int> FindSinkInputIndices(string id)
    {
        string json = RunCommand(PactlPath!, "-f json list sink-inputs");
        return ParseSinkInputEntries(json)
            .Where(entry => entry.Id == id)
            .Select(entry => entry.Index);
    }

    private static IReadOnlyList<AppAudioSession> ParseSinkInputs(string json)
    {
        return ParseSinkInputEntries(json)
            .GroupBy(entry => entry.Id)
            .Select(group => new AppAudioSession(
                group.Key,
                group.First().Name,
                group.Max(entry => entry.Volume),
                group.All(entry => entry.IsMuted)))
            .OrderBy(session => session.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<(int Index, string Id, string Name, int Volume, bool IsMuted)> ParseSinkInputEntries(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) yield break;

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array) yield break;

        foreach (var input in document.RootElement.EnumerateArray())
        {
            if (!input.TryGetProperty("index", out var indexElement)) continue;
            if (!input.TryGetProperty("properties", out var properties)) continue;

            string name = ReadString(properties, "application.name") ?? "Unknown";
            string id = (ReadString(properties, "application.process.binary") ?? name).ToLowerInvariant();

            yield return (
                indexElement.GetInt32(),
                id,
                name,
                ReadVolumePercent(input),
                input.TryGetProperty("mute", out var mute) && mute.GetBoolean());
        }
    }

    private static string? ReadString(JsonElement properties, string key) =>
        properties.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int ReadVolumePercent(JsonElement input)
    {
        if (!input.TryGetProperty("volume", out var volume) || volume.ValueKind != JsonValueKind.Object) return 0;

        int max = 0;
        foreach (var channel in volume.EnumerateObject())
        {
            if (!channel.Value.TryGetProperty("value_percent", out var percent)) continue;
            string text = percent.GetString()?.TrimEnd('%').Trim() ?? "";
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                max = Math.Max(max, value);
            }
        }

        return Math.Clamp(max, 0, 100);
    }
}
