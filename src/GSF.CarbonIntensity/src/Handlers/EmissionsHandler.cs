using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Exceptions;
using Microsoft.Extensions.Logging;

namespace GSF.CarbonIntensity.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    private readonly ICarbonAwareAggregator _aggregator;

    public EmissionsHandler(ILogger<EmissionsHandler> logger, ICarbonAwareAggregator aggregator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end)
    {
        var parameters = new CarbonAwareParametersBaseDTO {
            Start = start,
            End = end,
            SingleLocation = location
        };
        try {
            var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
            _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", result);
            return result;
        } catch (Exception e) {
            throw new CarbonIntensityException(e.Message, e);
        }

    }
}