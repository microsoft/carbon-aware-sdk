using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.LocationSources.Configuration;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocationSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        var _ = services.AddHttpClient<IpStackClient>(IIpStackClient.NamedClient);
        // TODO add proxy config for this client.
        services.TryAddSingleton<IIpStackClient, IpStackClient>();
        services.TryAddSingleton<ILocationSource, AutoLocationSource>();
        return services;
    }
}
