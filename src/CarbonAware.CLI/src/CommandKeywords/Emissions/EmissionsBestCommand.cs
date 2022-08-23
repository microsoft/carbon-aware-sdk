using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using System.CommandLine;
using System.Text.Json;


namespace CarbonAware.CLI.CommandKeywords.Emissions;

/// <summary>
/// Defines the command to help get a list of emission data by location for a specified time period.
/// </summary>
/// <argument name="location"> String named location.</argument>
/// <option name="startTime"> [Optional] Start time for the data query.</option>
/// <option name="endTime"> [Optional] End time for the data query.</option>
/// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
public static class EmissionsBestCommand
{
    static ICarbonAwareAggregator? _aggregator;
    public static void AddBestEmissionsCommand(this Command parent, ICarbonAwareAggregator aggregator)
    {
        _aggregator = aggregator;

        var command = TokenBuilder.Instance.CreateEmissionsBestCommand();
        parent.AddCommand(command);

        // Define options and arguments
        var locationsArgument = TokenBuilder.Instance.CreateLocationsArgument();
        var startTimeOption = TokenBuilder.Instance.CreateStartTimeOption();
        var endTimeOption = TokenBuilder.Instance.CreateEndTimeOption();

        command.AddArgument(locationsArgument);
        command.AddOption(startTimeOption);
        command.AddOption(endTimeOption);

        // Define handler 
        command.SetHandler(async (locations, startTime, endTime) =>
        {
            var result = await ListBestEmissions(locations, startTime, endTime);
            var outputData = $"{JsonSerializer.Serialize(result)}";
            Console.WriteLine(outputData);
        }, locationsArgument, startTimeOption, endTimeOption);

    }

    private static async Task<EmissionsData?> ListBestEmissions(string[] locations, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null, bool best = false)
    {
        CarbonAwareParametersBaseDTO parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = locations,
            Start = startTime,
            End = endTime
        };
        return await _aggregator!.GetBestEmissionsDataAsync(parameters);
    }
}

