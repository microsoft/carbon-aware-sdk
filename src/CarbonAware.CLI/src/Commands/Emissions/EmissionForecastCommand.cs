using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using CarbonAware.Tools.WattTimeClient.Model;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.Emissions;

class EmissionsForecastCommand : Command
{
    private readonly Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    
    private readonly Option<DateTimeOffset?> _dataEndTime = new Option<DateTimeOffset?>(
                new string[] { "--data-end-time", "-e" },
            LocalizableStrings.DataEndAtDecsription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<DateTimeOffset?> _dataRequestedAt = new Option<DateTimeOffset?>(
                new string[] { "--data-requested-at", "-r" },
            LocalizableStrings.DataRequestedAtDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<DateTimeOffset?> _dataStartTime = new Option<DateTimeOffset?>(
            new string[] { "--data-start-time", "-s" },
            LocalizableStrings.DataStartAtDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };
    private readonly Option<int?> _duration = new Option<int?>(
                new string[] { "--duration", "-d" },
            LocalizableStrings.DurationDescription)
    {
        Arity = ArgumentArity.ZeroOrOne,
    };

    public EmissionsForecastCommand() : base("emissions-forecast", LocalizableStrings.EmissionsCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_dataStartTime);
        AddOption(_dataEndTime);
        AddOption(_duration);
        AddOption(_dataRequestedAt);

        this.SetHandler(this.Run);
    }

    internal async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataStartTime);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataEndTime);
        var requestedAt = context.ParseResult.GetValueForOption<DateTimeOffset?>(_dataRequestedAt);
        var duration = context.ParseResult.GetValueForOption<int?>(_duration);

        // Call the aggregator
        var forecastParameters = new CarbonAwareParametersBaseDTO()
        {
            Start = startTime,
            End = endTime,
            Duration = duration
        };

        List<EmissionsForecastDTO> emissionsForecast = new();

        // If requestedAt is not provided, fetch the current forecast
        if (requestedAt != null)
        {
            forecastParameters.Requested = requestedAt;
            foreach (var location in locations!)
            {
                forecastParameters.SingleLocation = location;
                var forecast = await aggregator.GetForecastDataAsync(forecastParameters);
                if (forecast != null)
                {
                    emissionsForecast.Add((EmissionsForecastDTO)forecast);
                }
            }
        }
        else
        {
            forecastParameters.MultipleLocations = locations;
            var results = await aggregator.GetCurrentForecastDataAsync(forecastParameters);
            if (results != null)
            {
               emissionsForecast = results.Select(forecast => (EmissionsForecastDTO)forecast).ToList();
            }
        }
        var serializedOuput = JsonSerializer.Serialize(emissionsForecast);
        context.Console.WriteLine(serializedOuput);
    }
}
