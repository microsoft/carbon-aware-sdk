using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Text.Json;

namespace CarbonAware.NewCLI.CommandKeywords.Emissions;

/// <summary>
/// Defines the command to help get a list of emission data by location for a specified time period.
/// </summary>
/// <option name="location"> String named location.</option>
/// <option name="time"> [Optional] Start time for the data query.</option>
/// <option name="toTime"> [Optional] End time for the data query.</option>
/// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
public static class EmissionsListCommand
{
    static ICarbonAwareAggregator? _aggregator;
    public static void AddEmissionsListCommand(this Command parent, ICarbonAwareAggregator aggregator)
    {
        _aggregator = aggregator;

        var listCommand = new Command("list", "Lists emission data for given locations and times.");
        
        var locationOption = new Option<string[]>("--locations")
        {
            Description = "Space separated strings of Locations to get emissions data for.",
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true,
            Arity = ArgumentArity.OneOrMore
        };

        var startTimeOption = new Option<DateTime?>("--startTime", getDefaultValue: () => null)
        {
            Description = "Start time for emissions data window",
            IsRequired = false
        };

        var toTimeOption = new Option<DateTime?>("--toTime", getDefaultValue: () => null)
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
        }, locationOption, startTimeOption, toTimeOption, bestOption);

        parent.AddCommand(listCommand);
    }

    private static async Task<IEnumerable<EmissionsData>> ListEmissions(string[] locations, DateTimeOffset? startTime = null, DateTimeOffset? toTime = null, bool best = false)
    {
        IEnumerable<Location> locationsProp = locations.Select(loc => new Location() { RegionName = loc, LocationType = LocationType.CloudProvider });
        var props = new Dictionary<string, object?>()
        {
                { CarbonAwareConstants.MultipleLocations, locationsProp },
                { CarbonAwareConstants.Start, startTime },
                { CarbonAwareConstants.End, toTime },
                { CarbonAwareConstants.Best, best }
        };

        return await _aggregator!.GetEmissionsDataAsync(props);
    }
}

