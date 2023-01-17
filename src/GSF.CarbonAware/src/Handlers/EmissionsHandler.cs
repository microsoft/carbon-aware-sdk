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
        /* try
         {
             var emissionsData = await _aggregator.GetEmissionsDataAsync(parameters);
             var result = emissionsData.Select(e => (Models.EmissionsData)e);
             return result;
         }
         catch (CarbonAwareException ex)
         {
             throw new Exceptions.CarbonAwareException(ex.Message, ex);
         }*/

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
        /* var parameters = new CarbonAwareParametersBaseDTO
         {
             Start = start,
             End = end,
             MultipleLocations = locations
         };
         try
         {
             var emissionsData = await _aggregator.GetBestEmissionsDataAsync(parameters);
             var result = emissionsData.Select(e => (EmissionsData)e);
             return result;
         }
         catch (CarbonAwareException ex)
         {
             throw new Exceptions.CarbonAwareException(ex.Message, ex);
         }*/

        return null;
    }

    /// <inheritdoc />
    public async Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end)
    {
        /* var parameters = new CarbonAwareParametersBaseDTO {
             Start = start,
             End = end,
             SingleLocation = location
         };

         try {
             var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
             _logger.LogDebug("calculated average carbon intensity: {carbonIntensity}", result);
             return result;
         }
         catch (CarbonAwareException ex)
         {
             throw new Exceptions.CarbonAwareException(ex.Message, ex);
         }*/
        return 0;
    }
}
