using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Configuration;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        // find all the Classes in the Assembly that implements AddEmissionServices method,
        // and added them here with the specific implementation class.
        var dataSources = configuration.GetSection(DataSourcesConfiguration.Key).Get<DataSourcesConfiguration>();
        dataSources.ConfigurationSection = configuration.GetSection($"{DataSourcesConfiguration.Key}:Configurations");
        dataSources.AssertValid();

        var carbonAwareConfig = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var forecastDataSource = GetDataSourceTypeFromValue(carbonAwareConfig?.ForecastDataSource);
        var emissionsDataSource = GetDataSourceTypeFromValue(carbonAwareConfig?.EmissionsDataSource);

        if (forecastDataSource == DataSourceType.None && emissionsDataSource == DataSourceType.None)
        {
            throw new ArgumentException("At least one data source must be specified in configuration");
        }

        switch (forecastDataSource)
        {
            case DataSourceType.JSON:
            {
                throw new ArgumentException("JSON data source is not supported for forecast data");
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeForecastDataSource(configuration);
                break;
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IForecastDataSource, NullForecastDataSource>();
                break;
            }
        }

        switch (emissionsDataSource)
        {
            case DataSourceType.JSON:
            {
                services.AddJsonEmissionsDataSource(dataSources);
                break;
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeEmissionsDataSource(configuration);
                break;
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IEmissionsDataSource, NullEmissionsDataSource>();
                break;
            }
        }

        return services;
    }

    private static DataSourceType GetDataSourceTypeFromValue(string? srcVal)
    {
        DataSourceType result;
        if (String.IsNullOrWhiteSpace(srcVal))
        {
            result = DataSourceType.None;
        }
        else if (!Enum.TryParse<DataSourceType>(srcVal, true, out result))
        {
            throw new ArgumentException($"Unknown data source type: {srcVal}");
        }
        return result;
    }
}
