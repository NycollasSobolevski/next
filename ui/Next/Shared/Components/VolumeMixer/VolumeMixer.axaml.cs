using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Next.Services.Audio;
using Next.Shared.Components.Modules;

namespace Next.Shared.Components.VolumeMixer;

public partial class VolumeMixer : UserControl
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(1500);

    private readonly IAudioService? _audioService;
    private readonly DispatcherTimer _timer = new() { Interval = PollInterval };
    private readonly Dictionary<string, SoundSlider> _rows = new();
    private bool _isRefreshing;

    private readonly record struct MixerSnapshot(int MasterVolume, bool MasterMuted, IReadOnlyList<AppAudioSession> Apps);

    public VolumeMixer()
    {
        InitializeComponent();

        _audioService = App.Services?.GetService<IAudioService>();

        MasterRow.VolumeChanged += (_, volume) => _audioService?.SetMasterVolume(volume);
        MasterRow.MuteToggled += (_, muted) => _ = Task.Run(() => _audioService?.SetMuted(muted));

        _timer.Tick += (_, _) => _ = RefreshAsync();
        Loaded += (_, _) =>
        {
            _timer.Start();
            _ = RefreshAsync();
        };
        Unloaded += (_, _) => _timer.Stop();
    }

    private async Task RefreshAsync()
    {
        var audio = _audioService;
        if (audio == null || _isRefreshing) return;

        _isRefreshing = true;
        try
        {
            var snapshot = await Task.Run(() => new MixerSnapshot(
                audio.GetMasterVolume(),
                audio.IsMuted(),
                audio.GetAppSessions()));

            ApplySnapshot(snapshot);
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine($"[VolumeMixer] Refresh failed: {ex.Message}");
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void ApplySnapshot(MixerSnapshot snapshot)
    {
        MasterRow.Refresh("Master", Math.Max(snapshot.MasterVolume, 0), snapshot.MasterMuted);

        var present = new HashSet<string>();
        foreach (var app in snapshot.Apps)
        {
            present.Add(app.Id);

            if (!_rows.TryGetValue(app.Id, out var row))
            {
                row = CreateRow(app.Id);
                _rows[app.Id] = row;
                AppList.Children.Add(row);
            }

            row.Refresh(app.Name, app.Volume, app.IsMuted);
        }

        foreach (var id in _rows.Keys.Where(id => !present.Contains(id)).ToList())
        {
            AppList.Children.Remove(_rows[id]);
            _rows.Remove(id);
        }
    }

    private SoundSlider CreateRow(string id)
    {
        var row = new SoundSlider();
        row.VolumeChanged += (_, volume) => _ = Task.Run(() => _audioService?.SetAppVolume(id, volume));
        row.MuteToggled += (_, muted) => _ = Task.Run(() => _audioService?.SetAppMuted(id, muted));
        return row;
    }
}
