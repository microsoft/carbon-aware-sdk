using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands.Emissions;

class EmissionsCommand : Command
{
    private Option<string[]> _requiredLocationOption = CommonOptions.RequiredLocationOption;
    private Option<DateTimeOffset?> _startTimeOption = CommonOptions.StartTimeOption;
    private Option<DateTimeOffset?> _endTimeOption = CommonOptions.EndTimeOption;
    public EmissionsCommand() : base("emissions", LocalizableStrings.EmissionsCommandDescription)
    {
        AddOption(_requiredLocationOption);
        AddOption(_startTimeOption);
        AddOption(_endTimeOption);
        this.SetHandler(this.Run);

        AddCommand(new EmissionsForecastCommand());
    }

    private async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocationOption);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_startTimeOption);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_endTimeOption);
        
        var parameters = new CarbonAwareParametersBaseDTO() { 
            MultipleLocations = locations,
            Start = startTime,
            End = endTime
        };

        // Call the aggregator.
        var results = await aggregator.GetEmissionsDataAsync(parameters);

        context.Console.WriteLine(JsonSerializer.Serialize(results));
        context.ExitCode = 0;
    }
}