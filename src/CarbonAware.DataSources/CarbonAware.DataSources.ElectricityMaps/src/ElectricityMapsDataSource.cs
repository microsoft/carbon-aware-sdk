using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CarbonAware.DataSources.ElectricityMaps;

/// <summary>
/// Represents a Electricity Maps data source.
/// </summary>
public class ElectricityMapsDataSource : IForecastDataSource
{
    public string Name => "ElectricityMapsDataSource";

    public string Description => throw new NotImplementedException();

    public string Author => throw new NotImplementedException();

    public string Version => throw new NotImplementedException();

    private ILogger<ElectricityMapsDataSource> Logger { get; }

    private IElectricityMapsClient ElectricityMapsClient { get; }

    private static readonly ActivitySource Activity = new ActivitySource(nameof(ElectricityMapsDataSource));

    private ILocationSource LocationSource { get; }

    public double MinSamplingWindow => 120; // 2hrs of data


    /// <summary>
    /// Creates a new instance of the <see cref="ElectricityMapsDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    /// <param name="client">The ElectricityMaps Client</param>
    /// <param name="locationSource">The location source to be used to convert a location name to geocoordinates.</param>
    public ElectricityMapsDataSource(ILogger<ElectricityMapsDataSource> logger, IElectricityMapsClient client, ILocationSource locationSource)
    {
        this.Logger = logger;
        this.ElectricityMapsClient = client;
        this.LocationSource = locationSource;
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        using (var activity = Activity.StartActivity())
        {
            ForecastedCarbonIntensityData forecast;
            var geolocation = await this.LocationSource.ToGeopositionLocationAsync(location);
            if (geolocation.Latitude != null && geolocation.Latitude != null)
                forecast = await this.ElectricityMapsClient.GetCurrentForecastAsync(geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "");
            else {
                forecast = await this.ElectricityMapsClient.GetCurrentForecastAsync(geolocation.Name ?? "");
            }

            var requestedAt = DateTimeOffset.UtcNow;        
            var emissionsForecast = (EmissionsForecast) forecast;
            var duration = emissionsForecast.GetDurationBetweenForecastDataPoints();
            emissionsForecast.Location = location;
            emissionsForecast.RequestedAt = requestedAt;
            emissionsForecast.ForecastData = emissionsForecast.ForecastData.Select(d => 
            {
                d.Location = location.Name;
                d.Duration = duration;
                return d;
            });

            return emissionsForecast;
        }
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        /*        using (var activity = Activity.StartActivity())
                {
                    //TODO: Call with actual method and parameters
                    var geolocation = await this.LocationSource.ToGeopositionLocationAsync(location);
                    var forecast = await this.ElectricityMapsClient.GetForecastOnDateAsync(geolocation.Latitude.ToString() ?? "", geolocation.Longitude.ToString() ?? "", requestedAt);
                    if (forecast == null)
                    {
                        throw new ArgumentException($"No forecast was generated at the requested time {requestedAt}");
                    }
                    // keep input from the user.
                    return (EmissionsForecast) forecast;
                }
        */
        await Task.Run(() => true);
        throw new NotImplementedException();
    }
}
