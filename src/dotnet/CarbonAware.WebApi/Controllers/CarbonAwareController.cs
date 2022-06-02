using CarbonAware.Model;
using CarbonAware.Aggregators.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using CarbonAware.WebApi.Models;
using System.Globalization;
using System.Net.Mime;

namespace CarbonAware.WebApi.Controllers;

[ApiController]
[Route("emissions")]
public class CarbonAwareController : ControllerBase
{
    private readonly ILogger<CarbonAwareController> _logger;
    private readonly ICarbonAwareAggregator _aggregator;

    public CarbonAwareController(ILogger<CarbonAwareController> logger, ICarbonAwareAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocations/best")]
    public async Task<IActionResult> GetBestEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        //The LocationType is hardcoded for now. Ideally this should be received from the request or configuration 
        IEnumerable<Location> locationEnumerable = locations.Select(loc => new Location()
                                                                            { RegionName = loc, 
                                                                            LocationType=LocationType.CloudProvider});
        var props = new Dictionary<string, object?>() {
            { CarbonAwareConstants.Locations, locationEnumerable },
            { CarbonAwareConstants.Start, time},
            { CarbonAwareConstants.End, toTime },
            { CarbonAwareConstants.Duration, durationMinutes },
            { CarbonAwareConstants.Best, true }
        };

        return await GetEmissionsDataAsync(props);
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocations")]
    public async Task<IActionResult> GetEmissionsDataForLocationsByTime([FromQuery(Name = "locations")] string[] locations, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
        IEnumerable<Location> locationEnumerable = locations.Select(loc => new Location(){ RegionName = loc, LocationType=LocationType.CloudProvider });
        var props = new Dictionary<string, object?>() {
            { CarbonAwareConstants.Locations, locationEnumerable },
            { CarbonAwareConstants.Start, time },
            { CarbonAwareConstants.End, toTime},
            { CarbonAwareConstants.Duration, durationMinutes },
        };
        
        return await GetEmissionsDataAsync(props);
    }

    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetEmissionsDataForLocationByTime(string location, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {;
        var locations = new List<Location>() { new Location() { RegionName = location, LocationType=LocationType.CloudProvider } };
        var props = new Dictionary<string, object?>() {
            { CarbonAwareConstants.Locations, locations },
            { CarbonAwareConstants.Start, time },
            { CarbonAwareConstants.End, toTime },
            { CarbonAwareConstants.Duration, durationMinutes },
        };
        
        return await GetEmissionsDataAsync(props);
    }

    /// <summary>
    /// Given a dictionary of properties, handles call to GetEmissionsDataAsync including logging and response handling.
    /// </summary>
    /// <param name="props"> Dictionary of properties to call plugin. </param>
    /// <returns>Result of the plugin call or resulting status response</returns>
    private async Task<IActionResult> GetEmissionsDataAsync(Dictionary<string, object?> props)
    {
        // NOTE: Any auth information would need to be redacted from logging
        _logger.LogInformation("Calling plugin GetEmissionsDataAsync with paylod {@props}", props);

        var response = await _aggregator.GetEmissionsDataAsync(props);
        return response.Any() ? Ok(response) : NoContent();
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    /// <summary> Gets the marginal carbon intensity value </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator to retrieve carbon intenstiy</returns>
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
    
        _logger.LogDebug("calling to aggregator to calculate the average carbon intensity with input: {input}", input);
        (var startTime, var endTime) = ParseTimeInterval(input.TimeInterval);
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = input.Location.RegionName } }},
            { CarbonAwareConstants.Start, startTime },
            { CarbonAwareConstants.End, endTime }
        };

        var carbonIntensity = await _aggregator.CalcEmissionsAverageAsync(props);

        SciScore score = new SciScore
        {
            MarginalCarbonIntensityValue = carbonIntensity,
        };
        _logger.LogDebug("calculated marginal carbon intensity: {score}", score);
        return Ok(score);
    }

    // Validate and parse time interval string into a tuple of (start, end) DateTimeOffsets.
    // Throws ArgumentException for invalid input.
    private (DateTimeOffset start, DateTimeOffset end) ParseTimeInterval(string timeInterval)
    {
        DateTimeOffset start;
        DateTimeOffset end;

        var timeIntervals = timeInterval.Split('/');
        // Check that the time interval was split into exactly 2 parts
        if(timeIntervals.Length != 2)
        {
            throw new ArgumentException(
                $"Invalid TimeInterval. Expected exactly 2 dates separated by '/', recieved: {timeInterval}"
            );
        }

        var rawStart = timeIntervals[0];
        var rawEnd = timeIntervals[1];

        if(!DateTimeOffset.TryParse(rawStart, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out start))
        {
            throw new ArgumentException($"Invalid TimeInterval. Could not parse start time: {rawStart}");
        }

        if(!DateTimeOffset.TryParse(rawEnd, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal, out end))
        {
            throw new ArgumentException($"Invalid TimeInterval. Could not parse end time: {rawEnd}");
        }

        if(start > end)
        {
            throw new ArgumentException($"Invalid TimeInterval. Start time must come before end time: {timeInterval}");
        }

        return (start, end);
    }

}