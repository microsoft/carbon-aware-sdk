using System.Globalization;
using System.Reflection;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.LocationSources.Azure;

/// <summary>
/// Reprsents an azure location source.
/// </summary>
public class AzureLocationSource : ILocationSource
{
    public string Name => "Azure Location Source";

    public string Description => "Location source that knows how to get and work with Azure location information.";

    private readonly ILogger<AzureLocationSource> _logger;

    private IDictionary<string, NamedGeoposition> namedGeopositions;

    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a new instance of the <see cref="AzureLocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public AzureLocationSource(ILogger<AzureLocationSource> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        namedGeopositions = new Dictionary<string, NamedGeoposition>();
    }

    public Location ToGeopositionLocation(Location location)
    {
        switch (location.LocationType)
        {
            case LocationType.Geoposition:
            {
                return location;
            }
            case LocationType.CloudProvider: 
            {
                if( location.CloudProvider != CloudProvider.Azure ) 
                {
                    throw new LocationConversionException($"Incorrect Cloud provider region. Expected Azure but found '{ location.CloudProvider }'");
                }
                
                return getGeoPositionLocationOrThrow(location);
            }
        }
        
        throw new LocationConversionException($"Location '{ location.CloudProvider }' cannot be converted to Geoposition. ");
    }

    private Location getGeoPositionLocationOrThrow(Location location)
    {
        loadRegionsFromFileIfNotPresent();
        
        NamedGeoposition geopositionLocation = namedGeopositions[location.RegionName ?? ""];    
        if(geopositionLocation == null) 
        {
            throw new ArgumentException($"Lat/long cannot be retrieved for region '{ location.RegionName }'");
        }
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Converted Azure Location named '{regionName}' to Geoposition Location at latitude '{latitude}'" 
                                + "and logitude '{longitude}'.", location.RegionName, geopositionLocation.Latitude, geopositionLocation.Longitude);
        }
        return new Location 
                {
                    LocationType = LocationType.Geoposition,
                    Latitude = Convert.ToDecimal(geopositionLocation.Latitude),
                    Longitude = Convert.ToDecimal(geopositionLocation.Longitude)
                };
    }

    protected virtual Dictionary<string, NamedGeoposition> LoadRegionsFromJson()
    {
        var data = ReadFromResource("CarbonAware.LocationSources.Azure.azure-regions.json");
        List<NamedGeoposition> regionList = JsonSerializer.Deserialize<List<NamedGeoposition>>(data, options) ?? new List<NamedGeoposition>();
        Dictionary<string, NamedGeoposition> namedGeopositions = new Dictionary<String, NamedGeoposition>();
        foreach(NamedGeoposition region in regionList) 
        {
            namedGeopositions.Add(region.RegionName, region);
        }
        return namedGeopositions;
    }

    private void loadRegionsFromFileIfNotPresent() {
        if(namedGeopositions == null || !namedGeopositions.Any())
        {
            namedGeopositions = LoadRegionsFromJson();
        }
    }
    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }

}