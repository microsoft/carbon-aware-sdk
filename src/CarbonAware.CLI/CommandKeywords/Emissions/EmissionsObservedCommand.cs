using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Reflection;
using System.Resources;
using System.Text.Json;

namespace CarbonAware.CLI.CommandKeywords.Emissions;

/// <summary>
/// Defines the command to help get a list of emission data by location for a specified time period.
/// </summary>
/// <argument name="location"> String named location.</argument>
/// <option name="startTime"> [Optional] Start time for the data query.</option>
/// <option name="endTime"> [Optional] End time for the data query.</option>
/// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
public static class EmissionsObservedCommand
{
    static ICarbonAwareAggregator? _aggregator;     
    public static void AddEmissionsObservedCommand(this Command parent, ICarbonAwareAggregator aggregator)
    {
        _aggregator = aggregator;

        var command = new Command("observed", "Lists observed emission data for given locations and times.");
        parent.AddCommand(command);

        // Define options and arguments
        var locationsArgument = TokenBuilder.CreateLocationsArgument();
        var startTimeOption = TokenBuilder.CreateStartTimeOption();
        var endTimeOption = TokenBuilder.CreateEndTimeOption();
        
        command.AddArgument(locationsArgument);
        command.AddOption(startTimeOption);
        command.AddOption(endTimeOption);

        // Define handler 
        command.SetHandler(async (locations, startTime, endTime) =>
        {
            var result = await ListEmissions(locations, startTime, endTime);
            var outputData = $"{JsonSerializer.Serialize(result)}";
            Console.WriteLine(outputData);
        }, locationsArgument, startTimeOption, endTimeOption);

    }

    private static async Task<IEnumerable<EmissionsData>> ListEmissions(string[] locations, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, bool best = false)
    {
        var locationsProp = locations.Select(loc => new Location() { RegionName = loc, LocationType = LocationType.CloudProvider });
        var props = new Dictionary<string, object?>()
        {
                { CarbonAwareConstants.MultipleLocations, locationsProp },
                { CarbonAwareConstants.Start, startTime },
                { CarbonAwareConstants.End, endTime }
        };
        return await _aggregator!.GetEmissionsDataAsync(props);     
    }
}

