using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Net;

namespace CarbonAware.Tools.WattTimeClient.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method to configure and add the WattTime client to the service collection.
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configuration">The configuration to use to configure the client.</param>
    /// <returns>The service collection with the configured client added.</returns>
    /// </summary>
    public static IServiceCollection ConfigureWattTimeClient(this IServiceCollection services, IConfiguration configuration)
    {

        var source = new ActivitySource("WattTimeClient");

        WattTimeClientConfiguration config = new WattTimeClientConfiguration();

        // configuring dependency injection to have config.
        services.Configure<WattTimeClientConfiguration>(c =>
        {
            configuration?.GetSection(WattTimeClientConfiguration.Key).Bind(c);
        });
        var proxyVars = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        if (proxyVars.UseWebProxy)
        {
            services.AddHttpClient<WattTimeClient>()
                .ConfigurePrimaryHttpMessageHandler(() => 
                     new HttpClientHandler {
                        Proxy = new WebProxy(proxyVars.WebProxyUrl),
                        UseProxy = true,
                        Credentials = new NetworkCredential(proxyVars.WebProxyUsername, proxyVars.WebProxyPassword)
                    }
                );
        } else
        {
            services.AddHttpClient<WattTimeClient>();
        }

        services.TryAddSingleton<IWattTimeClient, WattTimeClient>();
        services.TryAddSingleton<ActivitySource>(source);

        return services;
    }
}
