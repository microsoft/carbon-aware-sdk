using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.DataSources.Configuration;
using CarbonAware.Aggregators.CarbonAware;


namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Service Extension that adds required services needed for pulling
    /// information from data sources that provide Carbon Intensity data.
    /// This method is aware of <see cref="EnvironmentVariables.CarbonIntensityDataSource"/>
    /// how it is set based on the values of <see cref="DataSourceType"/>
    /// For instance, setting "CARBON_INTENSITY_DATASOURCE=JSON" environment variable
    /// it would pull Carbon Intensity data from the static json file.
    /// If there "CARBON_INTENSITY_DATASOURCE" is not set or empty, it would default to JSON.
    /// </summary>
    /// 
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services)
    {
        var dsrc = Environment.GetEnvironmentVariable(EnvironmentVariables.CarbonIntensityDataSource);
        services.AddDataSourceService(GetDataSourceTypeFromValue(dsrc));
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
    }

    private static DataSourceType GetDataSourceTypeFromValue(string? envValue)
    {
        DataSourceType result;
        if (String.IsNullOrEmpty(envValue) ||
            !Enum.TryParse<DataSourceType>(envValue, true, out result))
        {
            // defaults to JSON in case env is empty, null or parsing fails
            return DataSourceType.JSON;
        }
        return result;
    }
}
