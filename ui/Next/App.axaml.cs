using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia;
using System;

using Next.Services;

namespace Next;

public partial class App : Application
{
    public static IServiceProvider Services { get; protected set; } = null!;
    public static Logger Logger = null!;
    public static Reader Reader = null!;
    public App()
    {
        var servicesCollection = new ServiceCollection();
        servicesCollection.ConfigureServices();
        Services = servicesCollection.BuildServiceProvider();
        Logger = Services.GetService<Logger>()!;
        Reader = Services.GetService<Reader>()!;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}