using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Services;
using Next.Services.Audio;

namespace Next;

public static partial class ServiceConfigurator
{
    public static IServiceCollection ConfigureServices(this ServiceCollection services)
    {
        services.AddSingleton<Logger>();
        services.AddSingleton<Reader>();
        services.AddSingleton<MainWindow>();

        services.AddSingleton<IAudioService>(sp =>
        {
            var logger = sp.GetService<Logger>();
            if (OperatingSystem.IsWindows())
            {
                return new WindowsAudioService(logger);
            }
            else if (OperatingSystem.IsLinux())
            {
                return new LinuxAudioService(logger);
            }
            else
            {
                return new NullAudioService(logger);
            }
        });

        return services;
    }
}
