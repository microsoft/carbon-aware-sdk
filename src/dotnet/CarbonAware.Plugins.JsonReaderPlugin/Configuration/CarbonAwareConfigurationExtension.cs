using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Plugins.JsonReaderPlugin.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddCarbonAwareServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
    }
}