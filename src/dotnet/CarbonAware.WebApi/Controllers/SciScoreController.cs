using CarbonAware.Aggregators.SciScore;
using CarbonAware.WebApi.Models;
using CarbonAware.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    public async Task<IActionResult> CreateAsync(SciScoreInput calculation)
    {
        if (calculation.Location == null)
        {
            return BadRequest("Location is required");
        }

        if (String.IsNullOrEmpty(calculation.TimeInterval))
        {
            return BadRequest("TimeInterval is required");
        }

        SciScore score = new SciScore
        {
            SciScoreValue = 100.0f,
            EnergyValue = 1.0f,
            MarginalCarbonEmissionsValue = 100.0f,
            EmbodiedEmissionsValue = 0.0f,
            FunctionalUnitValue = 1
        };

        return await Task.Run(() => Ok(score));
    }

    [HttpPost("marginal-carbon-intensity")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCarbonIntensityAsync(SciScoreInput calculation)
    {
        _logger.LogInformation(" calling to aggregator to ");
        var carbonIntensity = await _aggregator.CalculateAverageCarbonIntensityAsync(calculation.Location, calculation.TimeInterval);

        SciScore score = new SciScore
        {
            MarginalCarbonEmissionsValue = carbonIntensity,
        };

        return Ok(score);
    }


}