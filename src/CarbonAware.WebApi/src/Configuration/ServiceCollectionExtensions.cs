using Microsoft.Extensions.Options;

namespace CarbonAware.WebApi.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddMonitoringAndTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        
        var envVars = configuration?.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var telemetryProvider = GetTelemetryProviderFromValue(envVars?.TelemetryProvider);
        var logger = CreateConsoleLogger(configuration);
        switch (telemetryProvider) {
            case TelemetryProviderType.ApplicationInsights:
            {
                if (!String.IsNullOrEmpty(configuration?["ApplicationInsights_Connection_String"])) 
                {
                    logger.LogInformation("Application Insights connection string found");
                    services.AddApplicationInsightsTelemetry();
                } 
                else if (!String.IsNullOrEmpty(configuration?["ApplicationInsights_Instrumentation_Key"])) 
                {
                    logger.LogInformation("Application Insights instrumentation key found");
                    services.AddApplicationInsightsTelemetry(configuration?["ApplicationInsights_Instrumentation_Key"]);
                } 
                else 
                {
                    logger.LogWarning("Application Insights configuration not provided or incorrect.");
                }
                break;   
            }
            case TelemetryProviderType.NotProvided:
            {
                break;
            }
          // Can be extended in the future to support a different provider like Zipkin, Prometheus etc 
        }

    }

    public static ILogger CreateConsoleLogger(IConfiguration? config)
    {
        var factory = LoggerFactory.Create(b => {
            b.AddConfiguration(config?.GetSection("Logging"));
            b.AddConsole();
        });
        return factory.CreateLogger<IServiceCollection>();
    }
    private static TelemetryProviderType GetTelemetryProviderFromValue(string? srcVal)
    {
        TelemetryProviderType result;
        if (String.IsNullOrEmpty(srcVal) ||
            !Enum.TryParse<TelemetryProviderType>(srcVal, true, out result))
        {
            return TelemetryProviderType.NotProvided;
        }
        return result;
    }
}