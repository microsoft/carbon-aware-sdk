using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Commands.Emissions;
using CarbonAware.CLI.Common;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace CarbonAware.CLI.Commands;

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
    }

    private async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var location = context.ParseResult.GetValueForOption<string[]>(CommonOptions.RequiredLocationOption) ?? null;
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(CommonOptions.StartTimeOption);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(CommonOptions.EndTimeOption);
        
        var parameters = new CarbonAwareParametersBaseDTO() { 
            MultipleLocations = location,
            Start = startTime,
            End = endTime
        };

        // Call the aggregator.
        var results = await aggregator.GetEmissionsDataAsync(parameters);
        foreach (var result in results)
        {
            context.Console.WriteLine(JsonSerializer.Serialize(result));
        }
        context.ExitCode = 0;
    }
}