using CarbonAware.DataSources.Co2Signal.Client;
using CarbonAware.DataSources.Co2Signal.Model;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;

namespace CarbonAware.DataSources.Co2Signal;

/// <summary>
/// Represents a CO2 Signal data source.
/// </summary>
public class Co2SignalDataSource : IEmissionsDataSource
{
    public string _name => "Co2SignalDataSource";

    public string _description => throw new NotImplementedException();

    public string _author => throw new NotImplementedException();

    public string _version => throw new NotImplementedException();

    private ILogger<Co2SignalDataSource> _logger { get; }

    private ICo2SignalClient _Co2SignalClient { get; }

    private ILocationSource _locationSource { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="Co2SignalDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The Co2Signal Client</param>
    /// <param name="locationSource">The location source to be used to convert a location name to geocoordinates.</param>
    public Co2SignalDataSource(ILogger<Co2SignalDataSource> logger, ICo2SignalClient client, ILocationSource locationSource)
    {
        this._logger = logger;
        this._Co2SignalClient = client;
        this._locationSource = locationSource;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        this._logger.LogDebug("Getting latest carbon intensity for locations {locations}.", locations, periodStartTime, periodEndTime);
        List<EmissionsData> result = new();
        foreach (var location in locations)
        {
            IEnumerable<EmissionsData> interimResult = await GetCarbonIntensityAsync(location, periodStartTime, periodEndTime);
            result.AddRange(interimResult);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var geolocation = await this._locationSource.ToGeopositionLocationAsync(location);
        LatestCarbonIntensityData data;
        if (geolocation.Latitude != null && geolocation.Latitude != null)
            data = await this._Co2SignalClient.GetLatestCarbonIntensityAsync(geolocation.LatitudeAsCultureInvariantString(), geolocation.LongitudeAsCultureInvariantString());
        else
        {
            data = await this._Co2SignalClient.GetLatestCarbonIntensityAsync(geolocation.Name ?? "");
        }
        
        return new List<EmissionsData>() { 
            LatestCarbonIntensityToEmissionsData(location, data)
        };
    }

    private EmissionsData LatestCarbonIntensityToEmissionsData(Location location, LatestCarbonIntensityData latestData)
    {
        return new EmissionsData {
            Rating = latestData.Data.Value,
            Location = location.Name ?? string.Empty,
            Time = latestData.Data.DateTime,
            Duration = TimeSpan.Zero
        };
    }
}
