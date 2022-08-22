using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Logging;
using CarbonAware.CLI.CommandKeywords.Emissions;
using System.Reflection;
using System.Resources;

namespace CarbonAware.CLI;

class CarbonAwareCLI
{
    public static async Task<int> Main(string[] args)
    {
        ServiceProvider serviceProvider = BootstrapServices();
        ResourceManager resourceManager = new ResourceManager("CarbonAware.CLI.CommandOptions", Assembly.GetExecutingAssembly());

        var rootCommand = new RootCommand()
        {
            Description = resourceManager.GetString("rootCommandDesc")
        };

        EmissionsRootCommand.AddEmissionsCommands(ref rootCommand, serviceProvider.GetRequiredService<ICarbonAwareAggregator>());

        return await rootCommand.InvokeAsync(args);

    }

    private static ServiceProvider BootstrapServices()
    {

        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        var config = configurationBuilder.Build();
        var services = new ServiceCollection();
        services.Configure<CarbonAwareVariablesConfiguration>(config.GetSection(CarbonAwareVariablesConfiguration.Key));
        services.AddSingleton<IConfiguration>(config);
        services.AddCarbonAwareEmissionServices(config);
        services.AddLogging(configure => configure.AddConsole());

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

}