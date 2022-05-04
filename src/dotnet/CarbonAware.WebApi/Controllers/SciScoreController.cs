using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Diagnostics;

namespace CarbonAware.WebApi.Controllers;

[Route("sci-scores")]
[ApiController]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;
    private readonly ISciScoreAggregator _aggregator;

    private readonly ActivitySource _activitySource;

    public SciScoreController(ILogger<SciScoreController> logger, ISciScoreAggregator aggregator, ActivitySource activitySource)
    {
        _logger = logger;
        _aggregator = aggregator;
        _activitySource = activitySource;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public Task<IActionResult> CreateAsync(SciScoreInput input)
    {
        _logger.LogDebug("calculate sciscore with input: {input}", input);
        if (input.Location == null)
        {
            var error = new CarbonAwareWebApiError() { Message = "Location is required" };
            _logger.LogError("calculation failed with error: {error}");
            return Task.FromResult<IActionResult>(BadRequest(error));
        }

        if (String.IsNullOrEmpty(input.TimeInterval))
        {
            var error = new CarbonAwareWebApiError() { Message = "TimeInterval is required" };
            _logger.LogError("calculation failed with error: {error}");
            return Task.FromResult<IActionResult>(BadRequest(error));
        }

        var score = new SciScore
        {
            SciScoreValue = 100.0,
            EnergyValue = 1.0,
            MarginalCarbonEmissionsValue = 100.0,
            EmbodiedEmissionsValue = 0.0,
            FunctionalUnitValue = 1
        };
        _logger.LogDebug("calculated sciscore values: {score}", score);
        return Task.FromResult<IActionResult>(Ok(score));
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
        using (var activity = _activitySource.StartActivity())
        {
            _logger.LogDebug("calling to aggregator to calculate the average carbon intensity with input: {input}", input);
            // check that there is some location passed in
            if (input.Location == null)
            {
                var error = new CarbonAwareWebApiError() { Message = "Location is required" };
                _logger.LogError("get carbon intensity failed with error: {error}", error);
                return BadRequest(error);
            }

            // check that there is a time interval passed in
            if (String.IsNullOrEmpty(input.TimeInterval))
            {
                var error = new CarbonAwareWebApiError() { Message = "TimeInterval is required" };
                _logger.LogError("get carbon intensity failed with error: {error}", error);
                return BadRequest(error);
            }
            try
            {
                var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(input.Location, input.TimeInterval);

                SciScore score = new SciScore
                {
                    MarginalCarbonEmissionsValue = carbonIntensity,
                };
                _logger.LogDebug("calculated marginal carbon intensity: {score}", score);
                return Ok(score);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occured during marginal calculation execution", ex);
                var error = new CarbonAwareWebApiError() { Message = ex.ToString() };
                return BadRequest(error);
            }
        }
    }


}