using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Next.Services.Audio;

public class LinuxAudioService : IAudioService
{
    private readonly Logger? _logger;
    private readonly string? _toolPath;
    private readonly ToolType _toolType;

    private readonly object _throttleLock = new();
    private int _targetVolume = -1;
    private bool _isProcessing = false;

    private enum ToolType
    {
        None,
        Wpctl,
        Pactl
    }

    public LinuxAudioService(Logger? logger = null)
    {
        _logger = logger;

        if (File.Exists("/usr/bin/wpctl"))
        {
            _toolPath = "/usr/bin/wpctl";
            _toolType = ToolType.Wpctl;
        }
        else if (File.Exists("/usr/bin/pactl"))
        {
            _toolPath = "/usr/bin/pactl";
            _toolType = ToolType.Pactl;
        }
        else
        {
            _toolPath = FindInPath("wpctl") ?? FindInPath("pactl");
            if (_toolPath != null)
            {
                _toolType = _toolPath.EndsWith("wpctl", StringComparison.OrdinalIgnoreCase) 
                    ? ToolType.Wpctl 
                    : ToolType.Pactl;
            }
            else
            {
                _toolType = ToolType.None;
            }
        }

        _logger?.WriteLine($"[LinuxAudioService] Initialized with backend: {_toolType} ({_toolPath})");
    }

    public int GetMasterVolume()
    {
        try
        {
            if (_toolType == ToolType.Wpctl && _toolPath != null)
            {
                string output = RunCommand(_toolPath, "get-volume @DEFAULT_AUDIO_SINK@");
                // Expected format: "Volume: 0.50" or "Volume: 0.50 [MUTED]"
                var match = Regex.Match(output, @"Volume:\s+([0-9.]+)");
                if (match.Success && float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float fraction))
                {
                    return Math.Clamp((int)Math.Round(fraction * 100f), 0, 100);
                }
            }
            else if (_toolType == ToolType.Pactl && _toolPath != null)
            {
                string output = RunCommand(_toolPath, "get-sink-volume @DEFAULT_SINK@");
                // Expected format: "... / 50% / ..."
                var match = Regex.Match(output, @"(\d+)%");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int percent))
                {
                    return Math.Clamp(percent, 0, 100);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error reading volume: {ex.Message}");
        }

        return -1;
    }

    public void SetMasterVolume(int volume)
    {
        int clampedVolume = Math.Clamp(volume, 0, 100);

        lock (_throttleLock)
        {
            _targetVolume = clampedVolume;
            if (_isProcessing)
            {
                return;
            }
            _isProcessing = true;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                int currentTarget;
                lock (_throttleLock)
                {
                    currentTarget = _targetVolume;
                }

                ApplyVolumeDirect(currentTarget);

                await Task.Delay(25);

                lock (_throttleLock)
                {
                    if (currentTarget == _targetVolume)
                    {
                        _isProcessing = false;
                        break;
                    }
                }
            }
        });
    }

    private void ApplyVolumeDirect(int volume)
    {
        try
        {
            if (_toolType == ToolType.Wpctl && _toolPath != null)
            {
                float fraction = volume / 100f;
                string arg = string.Format(CultureInfo.InvariantCulture, "set-volume @DEFAULT_AUDIO_SINK@ {0:0.00}", fraction);
                RunCommand(_toolPath, arg);
                _logger?.WriteLine($"[LinuxAudioService] Set volume to {volume}% via wpctl");
            }
            else if (_toolType == ToolType.Pactl && _toolPath != null)
            {
                string arg = $"set-sink-volume @DEFAULT_SINK@ {volume}%";
                RunCommand(_toolPath, arg);
                _logger?.WriteLine($"[LinuxAudioService] Set volume to {volume}% via pactl");
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error setting volume to {volume}%: {ex.Message}");
        }
    }

    public bool IsMuted()
    {
        try
        {
            if (_toolType == ToolType.Wpctl && _toolPath != null)
            {
                string output = RunCommand(_toolPath, "get-volume @DEFAULT_AUDIO_SINK@");
                return output.Contains("[MUTED]", StringComparison.OrdinalIgnoreCase);
            }
            else if (_toolType == ToolType.Pactl && _toolPath != null)
            {
                string output = RunCommand(_toolPath, "get-sink-mute @DEFAULT_SINK@");
                return output.Contains("yes", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error checking mute status: {ex.Message}");
        }

        return false;
    }

    public void SetMuted(bool isMuted)
    {
        try
        {
            if (_toolType == ToolType.Wpctl && _toolPath != null)
            {
                RunCommand(_toolPath, $"set-mute @DEFAULT_AUDIO_SINK@ {(isMuted ? "1" : "0")}");
            }
            else if (_toolType == ToolType.Pactl && _toolPath != null)
            {
                RunCommand(_toolPath, $"set-sink-mute @DEFAULT_SINK@ {(isMuted ? "1" : "0")}");
            }
            _logger?.WriteLine($"[LinuxAudioService] Set muted: {isMuted}");
        }
        catch (Exception ex)
        {
            _logger?.WriteLine($"[LinuxAudioService] Error setting mute status: {ex.Message}");
        }
    }

    private static string RunCommand(string command, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) 
            ?? throw new InvalidOperationException($"Failed to start process: {command}");

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(1000);
        return output;
    }

    private static string? FindInPath(string toolName)
    {
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return null;

        var paths = pathEnv.Split(Path.PathSeparator);
        foreach (var dir in paths)
        {
            try
            {
                string fullPath = Path.Combine(dir, toolName);
                if (File.Exists(fullPath)) return fullPath;
            }
            catch
            {
                // Ignore path errors
            }
        }

        return null;
    }
}
