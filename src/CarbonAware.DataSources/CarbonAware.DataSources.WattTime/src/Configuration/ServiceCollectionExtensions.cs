using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Azure;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CarbonAware.DataSources.WattTime.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWattTimeForecastDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddDependencies(services, dataSourcesConfig);
        services.TryAddSingleton<IForecastDataSource, WattTimeDataSource>();

        return services;
    }

    public static IServiceCollection AddWattTimeEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        
        AddDependencies(services, dataSourcesConfig);
        services.TryAddSingleton<IEmissionsDataSource, WattTimeDataSource>();

        return services;
    }
    
    private static void AddDependencies(IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddWattTimeClient(services, dataSourcesConfig);
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
        services.AddMemoryCache();
    }

    private static void AddWattTimeClient(IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        services.Configure<WattTimeClientConfiguration>(c =>
        {
            dataSourcesConfig.EmissionsConfigurationSection().Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<WattTimeClient>(IWattTimeClient.NamedClient);

        var Proxy = dataSourcesConfig.EmissionsConfigurationSection().GetSection("Proxy").Get<WebProxyConfiguration>();
        if (Proxy != null && Proxy.UseProxy == true)
        {
            if (String.IsNullOrEmpty(Proxy.Url))
            {
                throw new ConfigurationException("Url is missing.");
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

        services.TryAddSingleton<IWattTimeClient, WattTimeClient>();
    }
}