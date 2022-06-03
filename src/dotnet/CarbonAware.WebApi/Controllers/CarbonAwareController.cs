using CarbonAware.Model;
using CarbonAware.Aggregators.CarbonAware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    /// <summary>
    /// Calculate the best emission data by location for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
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

    /// <summary>
    /// Calculate the observed emission data by list of locations for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocations", Name = "GetEmissionsDataForLocationsByTime") ]
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

    /// <summary>
    /// Calculate the best emission data by location for a specified time period.
    /// </summary>
    /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmissionsData>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("bylocation")]
    public async Task<IActionResult> GetEmissionsDataForLocationByTime([FromQuery, BindRequired] string location, DateTime? time = null, DateTime? toTime = null, int durationMinutes = 0)
    {
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
}