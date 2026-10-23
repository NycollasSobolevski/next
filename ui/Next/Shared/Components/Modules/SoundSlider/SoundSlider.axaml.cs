using System;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Next.Services;
using Next.Services.Audio;

namespace Next.Shared.Components.Modules;

public partial class SoundSlider : Module
{
    private IAudioService? _audioService;
    private bool _isUpdatingInternally;

    public int CurrentValue { get; set; } = 50;

    public SoundSlider() : base("Slider Module 01", 0)
    {
        InitializeComponent();
        ModuleLabel.Text = ModuleName;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _audioService = App.Services?.GetService<IAudioService>();

        if (_audioService != null)
        {
            try
            {
                int osVolume = _audioService.GetMasterVolume();
                if (osVolume >= 0)
                {
                    CurrentValue = osVolume;
                    App.Logger?.WriteLine($"[SoundSlider] Loaded OS volume: {CurrentValue}%");
                }
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine($"[SoundSlider] Failed to get initial OS volume: {ex.Message}");
            }
        }

        SetInternalSliderValue(CurrentValue);
    }

    public void OnChange(object sender, RangeBaseValueChangedEventArgs args)
    {
        if (_isUpdatingInternally) return;

        CurrentValue = (int)Math.Round(args.NewValue);
        currnentValueText.Text = $"{CurrentValue}%";

        _audioService ??= App.Services?.GetService<IAudioService>();
        _audioService?.SetMasterVolume(CurrentValue);
    }

    /// <summary>
    /// Updates the slider value from an external source (such as the physical hardware module)
    /// and synchronizes the change with the OS master volume.
    /// </summary>
    public void UpdateVolumeFromModule(int volume)
    {
        int clamped = Math.Clamp(volume, 0, 100);

        void Apply()
        {
            SetInternalSliderValue(clamped);
            _audioService ??= App.Services?.GetService<IAudioService>();
            _audioService?.SetMasterVolume(clamped);
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Apply();
        }
        else
        {
            Dispatcher.UIThread.Post(Apply);
        }
    }

    private void SetInternalSliderValue(int value)
    {
        _isUpdatingInternally = true;
        try
        {
            CurrentValue = value;
            SliderValue.Value = value;
            currnentValueText.Text = $"{value}%";
        }
        finally
        {
            _isUpdatingInternally = false;
        }
    }
}
