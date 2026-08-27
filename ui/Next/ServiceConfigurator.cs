using Microsoft.Extensions.DependencyInjection;
using Next.Services;

namespace Next;

public static partial class ServiceConfigurator
{
    public static IServiceCollection ConfigureServices(this ServiceCollection services)
    {
        services.AddSingleton<Logger>();

        return services;
    }
}