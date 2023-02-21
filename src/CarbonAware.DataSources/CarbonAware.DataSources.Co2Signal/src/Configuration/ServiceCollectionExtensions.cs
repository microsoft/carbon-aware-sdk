using CarbonAware.Configuration;
using CarbonAware.DataSources.Co2Signal.Client;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CarbonAware.DataSources.Co2Signal.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCo2SignalEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddCo2SignalClient(services, dataSourcesConfig.EmissionsConfigurationSection());
        services.TryAddSingleton<IEmissionsDataSource, Co2SignalDataSource>();
        return services;
    }
    
    private static void AddCo2SignalClient(IServiceCollection services, IConfigurationSection configSection)
    {
        services.Configure<Co2SignalClientConfiguration>(c =>
        {
            configSection.Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<Co2SignalClient>(ICo2SignalClient.NamedClient);

        var Proxy = configSection.GetSection("Proxy").Get<WebProxyConfiguration>();
        if (Proxy?.UseProxy == true)
        {
            if (String.IsNullOrEmpty(Proxy.Url))
            {
                throw new ConfigurationException("Proxy Url is not configured.");
            }
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => 
                new HttpClientHandler() {
                    Proxy = new WebProxy {
                        Address = new Uri(Proxy.Url),
                        Credentials = new NetworkCredential(Proxy.Username, Proxy.Password),
                        BypassProxyOnLocal = true
                    }
                }
            );
        }
        services.TryAddSingleton<ICo2SignalClient, Co2SignalClient>();
    }
}
