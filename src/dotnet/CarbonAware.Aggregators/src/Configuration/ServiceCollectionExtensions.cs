using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.DataSources.Configuration;
using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services)
    {
        services.AddDataSourceService(DataSourceType.WattTime);
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
    }
}