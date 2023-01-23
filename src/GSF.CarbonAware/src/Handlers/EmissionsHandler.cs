using CarbonAware.Exceptions;
using CarbonAware.Extensions;
using CarbonAware.Interfaces;
using GSF.CarbonAware.Handlers.CarbonAware;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;
using static GSF.CarbonAware.Handlers.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonAware.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    // private readonly IEmissionsAggregator _aggregator;
    private readonly IEmissionsDataSource _emissionsDataSource;
    /// <summary>
    /// Creates a new instance of the <see cref="EmissionsHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="aggregator">An <see cref="IEmissionsAggregator"> aggregator.</param>
    public EmissionsHandler(ILogger<EmissionsHandler> logger, IEmissionsDataSource emissionsDataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emissionsDataSource = emissionsDataSource ?? throw new ArgumentNullException(nameof(emissionsDataSource));
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetEmissionsDataAsync(new string[] { location }, start, end);
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var multipleLocations = parameters.MultipleLocations;
            var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var endTime = parameters.GetEndOrDefault(startTime);

            var emissionsData = await _emissionsDataSource.GetCarbonIntensityAsync(multipleLocations, startTime, endTime);

            return emissionsData.Select(e => (EmissionsData) e);
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetBestEmissionsDataAsync(new string[] { location }, start, end);   
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var endTime = parameters.GetEndOrDefault(startTime);
            var results = await _emissionsDataSource.GetCarbonIntensityAsync(parameters.MultipleLocations, startTime, endTime);
            var emissions = CarbonAwareOptimalEmission.GetOptimalEmissions(results).Select(r => (EmissionsData)r);
            return emissions;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
        
        
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end)
    {
        var dto = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            SingleLocation = location,
        };

        var parameters = (CarbonAwareParameters)dto;
        try
        {
            parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
            parameters.Validate();

            _logger.LogInformation("Aggregator getting average carbon intensity from data source");
            var emissionData = await _emissionsDataSource.GetCarbonIntensityAsync(parameters.SingleLocation, parameters.Start, parameters.End);
            var value = emissionData.AverageOverPeriod(start, end);
            _logger.LogInformation("Carbon Intensity Average: {value}", value);

            return value;
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
    }
}
