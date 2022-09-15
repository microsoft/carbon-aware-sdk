using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using CarbonAware.Model;
using System.Collections;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using System.Text.Json;

namespace CarbonAware.CLI.Commands.Emissions;

class EmissionsCommand : Command
{
    private readonly Option<string[]> _requiredLocation = CommonOptions.RequiredLocationOption;
    private readonly Option<DateTimeOffset?> _startTime = new Option<DateTimeOffset?>(
            new string[] { "--start-time", "-s" },
            LocalizableStrings.StartTimeDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<DateTimeOffset?> _endTime = new Option<DateTimeOffset?>(
             new string[] { "--end-time", "-e" },
            LocalizableStrings.EndTimeDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<bool> _best = new Option<bool>(
            new string[] { "--best", "-b" },
            LocalizableStrings.BestDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<bool> _average = new Option<bool>(
            new string[] { "--average", "-a" },
           LocalizableStrings.AverageDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    public EmissionsCommand() : base("emissions", LocalizableStrings.EmissionsCommandDescription)
    {
        AddOption(_requiredLocation);
        AddOption(_startTime);
        AddOption(_endTime);
        AddOption(_best);
        AddOption(_average);

        AddValidator(ValidateOptions);

        this.SetHandler(this.Run);
    }

    private void ValidateOptions(CommandResult commandResult)
    {
        // Validate mutually exclusive options 
        var average = commandResult.GetValueForOption<bool>(_average);
        var best = commandResult.GetValueForOption<bool>(_best);
        if (average && best)
        {
            commandResult.ErrorMessage = "Options --average and --best cannot be used together";
        }
    }
    internal async Task Run(InvocationContext context)
    {
        // Get aggregator via DI.
        var serviceProvider = context.BindingContext.GetService(typeof(IServiceProvider)) as IServiceProvider ?? throw new NullReferenceException("ServiceProvider not found");
        var aggregator = serviceProvider.GetService(typeof(ICarbonAwareAggregator)) as ICarbonAwareAggregator ?? throw new NullReferenceException("CarbonAwareAggregator not found");

        // Get the arguments and options to build the parameters.
        var locations = context.ParseResult.GetValueForOption<string[]>(_requiredLocation);
        var startTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_startTime);
        var endTime = context.ParseResult.GetValueForOption<DateTimeOffset?>(_endTime);
        var best = context.ParseResult.GetValueForOption<bool>(_best);
        var average = context.ParseResult.GetValueForOption<bool>(_average);
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            Start = startTime,
            End = endTime
        };
        // Call the aggregator.
        if (best)
        {
            parameters.MultipleLocations = locations;

            var result = await aggregator.GetBestEmissionsDataAsync(parameters);
           
            if(result != null)
            {
                var serializedOuput = JsonSerializer.Serialize((EmissionsDataDTO)result);
                context.Console.WriteLine(serializedOuput);
            }
        }
        else if (average) 
        {
            List<EmissionsDataDTO> emissions = new();

            foreach (var location in locations!)
            {
                parameters.SingleLocation = location;

                var averageCarbonIntensity = await aggregator.CalculateAverageCarbonIntensityAsync(parameters);
                
                // If startTime or endTime were not provided, the aggregator would have thrwon an error. So, at this point it is safe to assume that the start/end values are not null. 
                var emissionData = new EmissionsDataDTO()
                {
                    Location = location,
                    Time = startTime,
                    Duration = endTime - startTime,
                    Rating = averageCarbonIntensity
                };

                emissions.Add(emissionData);
            }
            var serializedOuput = JsonSerializer.Serialize(emissions);
            context.Console.WriteLine(serializedOuput);
        }
        else
        {
            parameters.MultipleLocations = locations;
            var results = await aggregator.GetEmissionsDataAsync(parameters);
            if (results != null)
            {
                var serializedOuput = JsonSerializer.Serialize((results.Select(emission => (EmissionsDataDTO)emission)));
                context.Console.WriteLine(serializedOuput);
            }
        }
        context.ExitCode = 0;
    }
}