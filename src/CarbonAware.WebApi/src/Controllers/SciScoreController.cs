using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

/// <summary>
/// Controller for the API routes that lead to retrieving the sci scores and marginal carbon intensities 
/// </summary>
[Route("sci-scores")]
[ApiController]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;
    private readonly ISciScoreAggregator _aggregator;

    private static readonly ActivitySource Activity = new ActivitySource(nameof(SciScoreController));

    public SciScoreController(ILogger<SciScoreController> logger, ISciScoreAggregator aggregator)
    {
        _logger = logger;
        _aggregator = aggregator;
    }

    /// <summary> Gets sci-score value, currently dummy function to keep consistency </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator that calculates the sci score</returns>
    /// <response code="200">successful operation</response>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(SciScore), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public Task<IActionResult> CreateAsync(SciScoreInput input)
    {
        _logger.LogDebug("calculate sciscore with input: {input}", input);

        var score = new SciScore
        {
            SciScoreValue = 100.0,
            EnergyValue = 1.0,
            MarginalCarbonIntensityValue = 100.0,
            EmbodiedEmissionsValue = 0.0,
            FunctionalUnitValue = 1
        };
        _logger.LogDebug("calculated sciscore values: {score}", score);
        return Task.FromResult<IActionResult>(Ok(score));
    }

    /// <summary> Gets the marginal carbon intensity value </summary>
    /// <param name="input"> input from JSON request converted to input object with location and time interval </param>
    /// <returns>Result of the call to the aggregator to retrieve carbon intenstiy</returns>

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
        using (var activity = Activity.StartActivity())
        {
            _logger.LogDebug("calling to aggregator to calculate the average carbon intensity with input: {input}", input);

            var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(GetLocation(input.Location), input.TimeInterval);

            SciScore score = new SciScore
            {
                MarginalCarbonIntensityValue = carbonIntensity,
            };
            _logger.LogDebug("calculated marginal carbon intensity: {score}", score);
            return Ok(score);
        }
    }

    /// <summary>
    /// Given an array of time intervals (with one given location), retrieve the actual carbon intensity that would have occurred 
    /// if the job was run at that time
    /// </summary>
    /// <remarks>
    /// This endpoint takes a batch of time intervals for actual carbon emissions data (for one given location), fetches them, and calculates the actual
    /// marginal carbon intensity values for the given list of time intervals to return the carbon intensity used during each interval.
    ///
    /// This endpoint is useful for figuring out how much carbon usage there would have been if the job was run at a specific date and time
    /// </remarks>
    /// <param name="requestActuals"> Array of requested actual values.</param>
    /// <returns>An array of actual carbon intensity values during the given time interval.</returns>
    /// <response code="200">Returns an array of responses which each contain a time interval and calculated carbon intensity values </response>
    /// <response code="400">Returned if any of the requested items are invalid</response>
    /// <response code="500">Internal server error</response>
    /// <response code="501">Returned if the underlying data source does not support getting the data needed to calculate sci-scores</response>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SciScore>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status501NotImplemented, Type = typeof(ValidationProblemDetails))]
    [HttpPost("marginal-carbon-intensity/batch")]
    public IActionResult BatchCarbonIntensityData(BatchSciScoreInput requestActuals)
    {
        // Dummy result.
        // TODO: implement this controller method after spec is approved.
        var result = new List<SciScore>();
        return Ok(result);
    }


    /// Validate the user input location and convert it to the internal Location object.
    //  Throws ArgumentException if input is invalid.
    private Location GetLocation(LocationInput locationInput)
    {
        LocationType locationType;
        CloudProvider cloudProvider;

        if (!Enum.TryParse<LocationType>(locationInput.LocationType, true, out locationType))
        {
            _logger.LogError("Can't parse location type with location input: ", locationInput);
            throw new ArgumentException($"locationType '{locationInput.LocationType}' is invalid");
        }

        Enum.TryParse<CloudProvider>(locationInput.CloudProvider, true, out cloudProvider);
        var location = new Location
        {
            LocationType = locationType,
            Latitude = locationInput.Latitude,
            Longitude = locationInput.Longitude,
            CloudProvider = cloudProvider,
            RegionName = locationInput.RegionName
        };

        return location;
    }


}