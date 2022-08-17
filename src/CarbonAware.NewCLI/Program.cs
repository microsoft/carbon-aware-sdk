using CarbonAware.NewCLI.CommandKeywords;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using Microsoft.Extensions.Logging;
using CarbonAware.NewCLI.CommandKeywords.Emissions;

namespace CarbonAware.NewCLI;

class Program
{
    public static int Main(string[] args)
    {
        ServiceProvider serviceProvider = BootstrapServices();

        var rootCommand = new RootCommand()
        {
            Description = "CLI for retrieving data using Carbonaware SDK"
        };

        EmissionsRootCommand.AddEmissionsCommands(ref rootCommand, serviceProvider.GetRequiredService<ICarbonAwareAggregator>());

        return rootCommand.Invoke(args);

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