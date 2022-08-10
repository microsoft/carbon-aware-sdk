using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Text.Json;

namespace CarbonAware.NewCLI.CommandKeywords;

public static class EmissionsCommand
{
    static ICarbonAwareAggregator? _aggregator;

    public static void AddEmissionsCommands(ref RootCommand rootCommandBase, ICarbonAwareAggregator aggregator)
    {
        var emissionsCommand = new Command("emissions", "Main Command for all Emissions related subcommands.");
        rootCommandBase.Add(emissionsCommand);

        _aggregator = aggregator;

        AddListSubcommand(ref emissionsCommand);

    }

    private static void AddListSubcommand(ref Command baseCommand)
    {
        var listCommand = new Command("list", "Lists emission data for given locations and times.");

        var locationOption = new Option<string[]>("--locations")
        {
            Description = "Space separated strings of Locations to get emissions data for.",
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
            Arity = ArgumentArity.OneOrMore
        };

        var startTimeOption = new Option<DateTime>("--startTime")
        {
            Description = "Start time for emissions data window",
            IsRequired = false
        };

        var toTimeOption = new Option<DateTime>("--toTime")
        {
            Description = "To Time for emissions data window",
            IsRequired = false
        };

        var bestOption = new Option<bool>("--best", getDefaultValue: () => false)
        {
            Description = "Filters to the best (lowest emission) window for each location",
            IsRequired = false
        };

        listCommand.AddOption(locationOption);
        listCommand.AddOption(startTimeOption);
        listCommand.AddOption(toTimeOption);
        listCommand.AddOption(bestOption);

        listCommand.SetHandler(async (locations, startTime, toTime, best) =>
        {
            var result = await ListEmissions(locations, startTime, toTime, best);
            var outputData = $"{JsonSerializer.Serialize(result)}";
            Console.WriteLine(outputData);
        },locationOption, startTimeOption, toTimeOption, bestOption);

        baseCommand.AddCommand(listCommand);
    }

    private static async Task<IEnumerable<EmissionsData>> ListEmissions(string[] locations, DateTime? startTime  = null, DateTime? toTime = null, bool best = false)
    {
        IEnumerable<Location> locationsProp = locations.Select(loc => new Location() { RegionName = loc, LocationType = LocationType.CloudProvider });
        var props = new Dictionary<string, object>() 
        {
                { CarbonAwareConstants.MultipleLocations, locationsProp },
                { CarbonAwareConstants.Start, startTime },
                { CarbonAwareConstants.End, toTime },
                { CarbonAwareConstants.Best, best }
        };

        return await _aggregator.GetEmissionsDataAsync(props);
    }

}