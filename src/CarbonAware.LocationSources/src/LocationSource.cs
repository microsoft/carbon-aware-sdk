using CarbonAware.Model;
using CarbonAware.Interfaces;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Options;
using CarbonAware.LocationSources.Configuration;

namespace CarbonAware.LocationSources;

/// <summary>
/// Represents a location source.
/// </summary>
public class LocationSource : ILocationSource
{
    private readonly ILogger<LocationSource> _logger;

    private IDictionary<string, NamedGeoposition> _namedGeopositions;

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private IOptionsMonitor<LocationDataSourcesConfiguration> _configurationMonitor { get; }

    private LocationDataSourcesConfiguration _configuration => _configurationMonitor.CurrentValue;


    /// <summary>
    /// Creates a new instance of the <see cref="LocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the LocationSource</param>
    public LocationSource(ILogger<LocationSource> logger, IOptionsMonitor<LocationDataSourcesConfiguration> monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _namedGeopositions = new Dictionary<string, NamedGeoposition>();
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        await LoadLocationFromFileIfNotPresentAsync();

        var regionName = location.RegionName ?? string.Empty;
        if (!_namedGeopositions.ContainsKey(regionName))
        {
            throw new ArgumentException($"Unknown region: Region name '{regionName}' not found");
        }

        NamedGeoposition geopositionLocation = _namedGeopositions[regionName];    
        if (!geopositionLocation.IsValidGeopositionLocation())  
        {
            throw new LocationConversionException($"Lat/long cannot be retrieved for region '{regionName}'");
        }
        return new Location
        {
            Latitude = Convert.ToDecimal(geopositionLocation.Latitude),
            Longitude = Convert.ToDecimal(geopositionLocation.Longitude)
        };
    }

    private async Task LoadLocationJsonFileAsync()
    {
        if (!_configuration.LocationSourceFiles.Any())
        {
            _logger.LogInformation($"Loading default location data source");
            await PopulateRegionMapAsync(new LocationSourceFile());
            return;
        }
        foreach (var data in _configuration.LocationSourceFiles)
        {
            await PopulateRegionMapAsync(data);
        }
    }

    private async Task PopulateRegionMapAsync(LocationSourceFile data)
    {
        using Stream stream = GetStreamFromFileLocation(data);
        var regionList = await JsonSerializer.DeserializeAsync<List<NamedGeoposition>>(stream, _options);
        foreach (var region in regionList!) 
        {
            _namedGeopositions.Add(BuildKeyFromRegion(data, region), region);
        }
    }

    private String BuildKeyFromRegion(LocationSourceFile locationData, NamedGeoposition region)
    {
        return $"{locationData.Prefix}{locationData.Delimiter}{region.RegionName}";
    }

    private async Task LoadLocationFromFileIfNotPresentAsync()
    {
        if (!_namedGeopositions.Any())
        {
            await LoadLocationJsonFileAsync();
        }
    }

    private Stream GetStreamFromFileLocation(LocationSourceFile locationData)
    {
        _logger.LogInformation($"Reading Location data source from {locationData.DataFileLocation}");
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Location data source Prefix '{locationData.Prefix}' and Delimiter '{locationData.Delimiter}'");
        }
        return File.OpenRead(locationData.DataFileLocation!);
    }
}
