using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Next.Services.Audio;

namespace Next.Shared.Components.Modules;

public partial class SoundSlider : Module
{
    public static readonly StyledProperty<IAudioService?> AudioServiceProperty =
        AvaloniaProperty.Register<SoundSlider, IAudioService?>(nameof(AudioService));

    public IAudioService? AudioService
    {
        get => GetValue(AudioServiceProperty);
        set => SetValue(AudioServiceProperty, value);
    }
    private bool _isUpdatingInternally;
    private bool _isDragging;

    public int CurrentValue { get; set; } = 50;
    public bool IsMuted { get; set; } = false;
    public string MutedImageAction { get; set; } = "avares://Next/Assets/Images/icons/sound_muted.png";
    public string UnmutedImageAction { get; set; } = "avares://Next/Assets/Images/icons/sound.png";

    public event EventHandler<int>? VolumeChanged;
    public event EventHandler<bool>? MuteToggled;

    public SoundSlider() : base("Slider Module 01", 0)
    {
        InitializeComponent();
        this.DataContext = this;
        // ModuleLabel.Text = ModuleName;

        SliderValue.AddHandler(InputElement.PointerPressedEvent, (_, _) => _isDragging = true, RoutingStrategies.Tunnel, handledEventsToo: true);
        SliderValue.AddHandler(InputElement.PointerReleasedEvent, (_, _) => _isDragging = false, RoutingStrategies.Bubble, handledEventsToo: true);
        SliderValue.PointerCaptureLost += (_, _) => _isDragging = false;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (AudioService != null)
        {
            try
            {
                int volume = AudioService.GetMasterVolume();
                if (volume >= 0)
                {
                    CurrentValue = volume;
                    IsMuted = AudioService.IsMuted();
                    App.Logger?.WriteLine($"[SoundSlider] Loaded volume: {CurrentValue}% ans Muted: {IsMuted}");
                }
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine($"[SoundSlider] Failed to get initial volume: {ex.Message}");
            }
        }
        

        UpdateActionIcon();
        SetInternalSliderValue(CurrentValue);
    }

    public void Refresh(string name, int volume, bool muted)
    {
        ModuleName = name;
        if (!_isDragging)
        {
            SetInternalSliderValue(volume);
        }
        if (IsMuted != muted)
        {
            IsMuted = muted;
            UpdateActionIcon();
        }
    }

    public void OnChange(object sender, RangeBaseValueChangedEventArgs args)
    {
        if (_isUpdatingInternally) return;

        CurrentValue = (int)Math.Round(args.NewValue);
        currnentValueText.Text = $"{CurrentValue}%";

        AudioService?.SetMasterVolume(CurrentValue);
        VolumeChanged?.Invoke(this, CurrentValue);
    }

    public void ActionButtonToggle(object sender, RoutedEventArgs args)
    {
        App.Logger?.WriteLine($"[SoundSlider] Action button clicked.");
        IsMuted = !(AudioService?.IsMuted() ?? IsMuted);
        AudioService?.SetMuted(IsMuted);
        MuteToggled?.Invoke(this, IsMuted);

        UpdateActionIcon();
    }

    private void UpdateActionIcon()
    {
        ActionButtonImage.Source = new Avalonia.Media.Imaging.Bitmap(
            AssetLoader.Open(
                new Uri(IsMuted ? MutedImageAction : UnmutedImageAction)
            )
        );
    }

    
    public void OnModuleLabelClick(object sender, RoutedEventArgs args)
    {
        
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
            AudioService ??= App.Services?.GetService<IAudioService>();
            AudioService?.SetMasterVolume(clamped);
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
