using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.CLI.Common;
using CarbonAware.CLI.Model;
using CarbonAware.Model;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
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
    private readonly Option<bool?> _best = new Option<bool?>(new string[] { "--best", "-b" },
            LocalizableStrings.BestDescription)
            {
                Arity = ArgumentArity.ZeroOrOne,
            };
    private readonly Option<bool?> _average = new Option<bool?>(new string[] { "--average", "-a" },
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

        ValidateMutuallyExclusiveOptions(_average, _best);

        this.SetHandler(this.Run);
    }

    private void ValidateMutuallyExclusiveOptions(Option<bool?> average, Option<bool?> best)
    {
        _average.AddValidator(r =>
        {
            if (r.FindResultFor(_best) is not null)
            {
                r.ErrorMessage = "Cannot have both options***********";
            }
        });
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
        var best = context.ParseResult.GetValueForOption<bool?>(_best);
        var average = context.ParseResult.GetValueForOption<bool?>(_average);


        // Call the aggregator.
        if (best == true)
        {
            var parameters = new CarbonAwareParametersBaseDTO()
            {
                MultipleLocations = locations,
                Start = startTime,
                End = endTime
            };
            var result = await aggregator.GetBestEmissionsDataAsync(parameters);
            
            context.Console.WriteLine(JsonSerializer.Serialize(ConvertToEmissionsDTO(result!)));
        }
        else if (average == true) 
        {
            List<EmissionsDataDTO> emissions = new(); 
            foreach (var location in locations!)
            {
                var parameters = new CarbonAwareParametersBaseDTO()
                {
                    SingleLocation = location,
                    Start = startTime,
                    End = endTime
                };
                var averageCarbonIntensity = await aggregator.CalculateAverageCarbonIntensityAsync(parameters);
                var emissionData = new EmissionsDataDTO()
                {
                    Location = location,
                    Rating = averageCarbonIntensity
                };

                emissions.Add(emissionData);
            }
           
            context.Console.WriteLine(JsonSerializer.Serialize(emissions));
        }
        else
        {
            var parameters = new CarbonAwareParametersBaseDTO()
            {
                MultipleLocations = locations,
                Start = startTime,
                End = endTime
            };
            var results = await aggregator.GetEmissionsDataAsync(parameters);
            var emissions = results.AsEnumerable<EmissionsData>().Select(e => ConvertToEmissionsDTO(e));
            context.Console.WriteLine(JsonSerializer.Serialize(emissions));
        }

        context.ExitCode = 0;
    }

    private EmissionsDataDTO ConvertToEmissionsDTO(EmissionsData emissionsData)
    {
        return new EmissionsDataDTO()
        {
            Location = emissionsData.Location,
            Time = emissionsData.Time,
            Rating =  emissionsData.Rating
        };
    }
}