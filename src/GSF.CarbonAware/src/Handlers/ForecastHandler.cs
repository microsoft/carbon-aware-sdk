
using CarbonAware.Interfaces;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;
using CarbonAwareException = CarbonAware.Exceptions.CarbonAwareException;

namespace GSF.CarbonAware.Handlers;

internal sealed class ForecastHandler : IForecastHandler
{
    private readonly IForecastDataSource _dataSource;
    private readonly ILogger<ForecastHandler> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="ForecastHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="aggregator">An <see cref="IForecastAggregator"> aggregator.</param>
    public ForecastHandler(ILogger<ForecastHandler> logger, IForecastDataSource _dataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataSource = _dataSource ?? throw new ArgumentNullException(nameof(_dataSource));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsForecast>> GetCurrentForecastAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null, int? duration = null)
    {
        /*var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
            Duration = duration
        };
        try {
            var forecasts = await _aggregator.GetCurrentForecastDataAsync(parameters);
            var result = forecasts.Select(f => (EmissionsForecast)f);
            _logger.LogDebug("Current forecast: {result}", result);
            return result;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }*/
        return null;
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetForecastByDateAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null, DateTimeOffset? requestedAt = null, int? duration = null)
    {
        /*var parameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            SingleLocation = location,
            Requested = requestedAt,
            Duration = duration
        };
        try
        {
            var forecast = await _aggregator.GetForecastDataAsync(parameters);
            var result = (EmissionsForecast)forecast;

            _logger.LogDebug("Forecast: {result}", result);
            return result;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }*/

        return null;
    }
}
