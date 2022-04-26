using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CarbonAware.WebApi.Controllers;

[Route("sci-scores")]
[ApiController]
public class SciScoreController : ControllerBase
{
    private readonly ILogger<SciScoreController> _logger;
    private readonly ISciScoreAggregator _aggregator;

    public SciScoreController(ILogger<SciScoreController> logger, ISciScoreAggregator aggregator)
    {
        _logger = logger;
        _aggregator = aggregator;
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> CreateAsync(SciScoreInput input)
    {
        if (input.Location == null)
        {
            return BadRequest("Location is required");
        }

        if (String.IsNullOrEmpty(input.TimeInterval))
        {
            return BadRequest("TimeInterval is required");
        }

        SciScore score = new SciScore
        {
            SciScoreValue = 100.0,
            EnergyValue = 1.0,
            MarginalCarbonEmissionsValue = 100.0,
            EmbodiedEmissionsValue = 0.0,
            FunctionalUnitValue = 1
        };

        return await Task.Run(() => Ok(score));
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput input)
    {
        _logger.LogInformation(" calling to aggregator to ");
        if (input.Location == null)
        {
            return BadRequest("Location is required");
        }

        if (String.IsNullOrEmpty(input.TimeInterval))
        {
            return BadRequest("TimeInterval is required");
        }
        try
        {
            var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(input.Location, input.TimeInterval);

            SciScore score = new SciScore
            {
                MarginalCarbonEmissionsValue = carbonIntensity,
            };

            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured during marginal calculation execution", ex);
            return BadRequest(ex.ToString());
        }

    }


}