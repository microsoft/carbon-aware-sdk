using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.DataSources.ElectricityMaps.Client;
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
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        throw new NotImplementedException();
    }
}