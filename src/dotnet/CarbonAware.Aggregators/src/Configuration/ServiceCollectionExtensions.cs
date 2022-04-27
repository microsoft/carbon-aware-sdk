using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.DataSources.Configuration;
using CarbonAware.Aggregators.CarbonAware;


namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
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
