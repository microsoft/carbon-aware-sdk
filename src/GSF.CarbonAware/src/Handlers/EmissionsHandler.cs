using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using GSF.CarbonAware.Handlers.CarbonAware;
using GSF.CarbonAware.Models;
using Microsoft.Extensions.Logging;
using static GSF.CarbonAware.Handlers.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonAware.Handlers;

internal sealed class EmissionsHandler : IEmissionsHandler
{
    private readonly ILogger<EmissionsHandler> _logger;
    private readonly IEmissionsDataSource _emissionsDataSource;
    /// <summary>
    /// Creates a new instance of the <see cref="EmissionsHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger for the handler</param>
    /// <param name="emissionsDataSource">An <see cref="IEmissionsDataSource"> datasource.</param>
    public EmissionsHandler(ILogger<EmissionsHandler> logger, IEmissionsDataSource emissionsDataSource)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emissionsDataSource = emissionsDataSource ?? throw new ArgumentNullException(nameof(emissionsDataSource));
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<Models.EmissionsData>> GetEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetEmissionsDataAsync(new string[] { location }, start, end);
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<Models.EmissionsData>> GetEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var carbonAwareParameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations
        };

        CarbonAwareParameters parameters = carbonAwareParameters;
        
        try
        {
            parameters.SetRequiredProperties(PropertyName.MultipleLocations);
            parameters.SetValidations(ValidationName.StartRequiredIfEnd);
            parameters.Validate();

            var multipleLocations = parameters.MultipleLocations;
            var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
            var endTime = parameters.GetEndOrDefault(startTime);

            var emissionsData = await _emissionsDataSource.GetCarbonIntensityAsync(multipleLocations, startTime, endTime);
            return emissionsData.Select(e => (Models.EmissionsData)e);
            
        }
        catch (CarbonAwareException ex)
        {
            throw new Exceptions.CarbonAwareException(ex.Message, ex);
        }
       
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<Models.EmissionsData>> GetBestEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        return await GetBestEmissionsDataAsync(new string[] { location }, start, end);   
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<Models.EmissionsData>> GetBestEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null)
    {
        var carbonAwareParameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            MultipleLocations = locations
        };
        CarbonAwareParameters parameters = carbonAwareParameters;

        parameters.SetRequiredProperties(PropertyName.MultipleLocations);
        parameters.SetValidations(ValidationName.StartRequiredIfEnd);
        parameters.Validate();

        var startTime = parameters.GetStartOrDefault(DateTimeOffset.UtcNow);
        var endTime = parameters.GetEndOrDefault(startTime);
        var results = await _emissionsDataSource.GetCarbonIntensityAsync(parameters.MultipleLocations, startTime, endTime);
        IEnumerable<Models.EmissionsData> emissions = results.Select(e => (Models.EmissionsData)e); ;
        return GetOptimalEmissions(emissions);
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end)
    {
        var carbonAwareParameters = new CarbonAwareParametersBaseDTO
        {
            Start = start,
            End = end,
            SingleLocation = location
        };
        CarbonAwareParameters parameters = carbonAwareParameters;
        parameters.SetRequiredProperties(PropertyName.SingleLocation, PropertyName.Start, PropertyName.End);
        parameters.Validate();

        
        _logger.LogInformation("Aggregator getting average carbon intensity from data source");
        var emissionData = await _emissionsDataSource.GetCarbonIntensityAsync(parameters.SingleLocation, parameters.Start, parameters.End);
        //TODO: Call actual method
        var value = 0;// emissionData.AverageOverPeriod(start, end);
        _logger.LogInformation("Carbon Intensity Average: {value}", value);

        return value;

    }

    private static IEnumerable<Models.EmissionsData> GetOptimalEmissions(IEnumerable<Models.EmissionsData> emissionsData)
    {
        if (!emissionsData.Any())
        {
            return Array.Empty<Models.EmissionsData>();
        }

        var bestResult = emissionsData.MinBy(x => x.Rating);

        IEnumerable<Models.EmissionsData> results = Array.Empty<Models.EmissionsData>();

        if (bestResult != null)
        {
            results = emissionsData.Where(x => x.Rating == bestResult.Rating);
        }

        return results;
    }
}
