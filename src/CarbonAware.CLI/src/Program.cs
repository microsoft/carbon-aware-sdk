using CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.CLI.Commands;
using CarbonAware.CLI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .Configure<CarbonAwareVariablesConfiguration>(
        config.GetSection(CarbonAwareVariablesConfiguration.Key))
    .AddCarbonAwareEmissionServices(config)
    .AddLogging(builder => builder.AddDebug())
    .BuildServiceProvider();

// TODO: Add localization
// https://github.com/dotnet/command-line-api/blob/main/src/System.CommandLine/LocalizationResources.cs
var rootCommand = new RootCommand();
rootCommand.AddCommand(new EmissionsCommand());

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseCarbonAwareExceptionHandler()
    .AddMiddleware(async (context, next) =>
        {
            context.BindingContext.AddService<IServiceProvider>(_ => serviceProvider);
            await next(context);
        }
    )
    .Build();

return await parser.InvokeAsync(args);
