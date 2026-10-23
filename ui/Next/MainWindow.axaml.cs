using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Next.Services.Audio;

namespace Next;

public partial class MainWindow : Window
{
    public List<IAudioService> AudioServices { get; } = [];
    public MainWindow()
    {
        InitializeComponent();

        this.DataContext = this;

        var AudioService = App.Services?.GetService<IAudioService>();
        if(AudioService != null)
        {
            AudioServices.Add(AudioService);
            AudioServices.Add(AudioService);
            AudioServices.Add(AudioService);
        }

    }
}