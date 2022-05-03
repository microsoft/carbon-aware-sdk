using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.DataSources.Configuration;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.SciScore;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services)
    {
        services.AddDataSourceService(DataSourceType.JSON);
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
    }

     public static void AddCarbonAwareSciScoreServices(this IServiceCollection services)
    {
        services.AddDataSourceService(DataSourceType.WattTime);
        services.TryAddSingleton<ISciScoreAggregator, SciScoreAggregator>();
    }
}