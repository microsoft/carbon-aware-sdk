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
    public string Name => "Location Source";

    public string Description => "Location source that knows how to get and work with location information.";

    private readonly ILogger<LocationSource> _logger;

    private IDictionary<string, NamedGeoposition>? namedGeopositions;

    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

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
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        switch (location.LocationType)
        {
            case LocationType.Geoposition:
            {
                return await Task.FromResult(location);
            }
            case LocationType.CloudProvider: 
            {
                var geoPositionLocation = await GetGeoPositionLocationOrThrowAsync(location);
                return geoPositionLocation;
            }
        }
        throw new LocationConversionException($"Location '{ location.LocationType }' cannot be converted to Geoposition. ");
    }

    private Task<Location> GetGeoPositionLocationOrThrowAsync(Location location)
    {
        LoadRegionsFromFileIfNotPresentAsync();

        var regionName = location.RegionName ?? string.Empty;
        if (! namedGeopositions!.ContainsKey(regionName))
        {
            throw new ArgumentException($"Unknown region: Region name '{regionName}' not found");
        }

        NamedGeoposition geopositionLocation = namedGeopositions![regionName];    
        if (!geopositionLocation.IsValidGeopositionLocation())  
        {
            throw new LocationConversionException($"Lat/long cannot be retrieved for region '{regionName}'");
        }
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Converted Azure Location named '{location.RegionName}' to Geoposition Location at latitude '{location.Latitude}'" 
                                + "and logitude '{longitude}'.", location.RegionName, geopositionLocation.Latitude, geopositionLocation.Longitude);
        }
        Location geoPosistionLocation = new Location 
                {
                    LocationType = LocationType.Geoposition,
                    Latitude = Convert.ToDecimal(geopositionLocation.Latitude),
                    Longitude = Convert.ToDecimal(geopositionLocation.Longitude)
                };

        return Task.FromResult(geoPosistionLocation);        
    }

    protected virtual async Task<Dictionary<String, NamedGeoposition>> LoadRegionsFromJsonAsync()
    {
        var regionGeopositionMapping = new Dictionary<String, NamedGeoposition>();
        if (_configuration.LocationDataSources is null || !_configuration.LocationDataSources.Any())
        {
            _logger.LogInformation($"Loading default location data configuration");
            await PopulateRegionMap(regionGeopositionMapping, LocationDataSource.DefaultAzureLocationDataSource());
        }
        else
        {
            foreach (var data in _configuration.LocationDataSources!)
            {
                await PopulateRegionMap(regionGeopositionMapping, data);
            }
        }
        return regionGeopositionMapping;
    }

    private async Task PopulateRegionMap(Dictionary<String, NamedGeoposition> map, LocationDataSource data)
    {
        using Stream stream = GetStreamFromFileLocation(data);
        var regionList = await JsonSerializer.DeserializeAsync<List<NamedGeoposition>>(stream, options) ?? new List<NamedGeoposition>();
        foreach (var region in regionList) 
        {
            map.Add(BuildRegionPrefix(data, region), region);
        }
    }

    private String BuildRegionPrefix(LocationDataSource locationData, NamedGeoposition region)
    {
        if (String.IsNullOrEmpty(locationData.Prefix) || locationData.Delimiter == null)
        {
            return region.RegionName;
        }
        return $"{locationData.Prefix}{locationData.Delimiter}{region.RegionName}";
    }

    private async void LoadRegionsFromFileIfNotPresentAsync() {
        if (namedGeopositions == null || !namedGeopositions.Any())
        {
            namedGeopositions = await LoadRegionsFromJsonAsync();
        }
    }

    private Stream GetStreamFromFileLocation(LocationDataSource locationData)
    {
        _logger.LogInformation($"Reading Location data from {locationData.DataFileLocation}");
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Location Data Prefix {locationData.Prefix} and Delimiter {locationData.Delimiter}");
        }
        return File.OpenRead(locationData.DataFileLocation!);
    }
}
