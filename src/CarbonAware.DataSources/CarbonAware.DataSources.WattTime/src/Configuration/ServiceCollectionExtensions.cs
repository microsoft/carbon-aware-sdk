using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.DataSources.WattTime.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddWattTimeForecastDataSource(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        services.ConfigureWattTimeClient(configuration);
        services.TryAddSingleton<IForecastDataSource, WattTimeDataSource>();
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
    }

    public static void AddWattTimeEmissionsDataSource(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        services.ConfigureWattTimeClient(configuration);
        services.TryAddSingleton<IEmissionsDataSource, WattTimeDataSource>();
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
    }
}