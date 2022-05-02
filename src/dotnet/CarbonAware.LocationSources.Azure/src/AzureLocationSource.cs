using System.Globalization;
using System.Reflection;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CarbonAware.LocationSources.Azure;

/// <summary>
/// Reprsents an azure location source.
/// </summary>
public class AzureLocationSource : ILocationSource
{
    public string Name => "Azure Location Source";

    public string Description => "Location source that knows how to get and work with Azure location information.";

    private readonly ILogger<AzureLocationSource> _logger;

    private IDictionary<string, NamedGeoposition> namedGeopositions = new Dictionary<string, NamedGeoposition>();

    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a new instance of the <see cref="AzureLocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public AzureLocationSource(ILogger<AzureLocationSource> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        namedGeopositions = LoadRegionsFromJson();
    }

    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }

    protected virtual Dictionary<string, NamedGeoposition>? LoadRegionsFromJson()
    {
        var data = ReadFromResource("CarbonAware.LocationSources.Azure.azure-regions.json");
        List<NamedGeoposition> regionList = JsonSerializer.Deserialize<List<NamedGeoposition>>(data, options) ?? new List<NamedGeoposition>();
        Dictionary<string, NamedGeoposition> namedGeopositions = new Dictionary<String, NamedGeoposition>();
        foreach(NamedGeoposition region in regionList) {
            namedGeopositions.Add(region.RegionName, region);
        }
        return namedGeopositions;
    }

    public Location ToGeopositionLocation(Location location)
    {
        switch (location.LocationType) {
            case LocationType.Geoposition: {
                return location;
            }
            case LocationType.CloudProvider: {
                if(location.CloudProvider != CloudProvider.Azure) {
                    throw new ArgumentException($"Incorrect Cloud provider region. Expected Azure but found '{ location.CloudProvider }'");
                }
                NamedGeoposition region = namedGeopositions[location.RegionName];

                return new Location {
                    LocationType = LocationType.Geoposition,
                    Latitude = Convert.ToDecimal(region.Latitude),
                    Longitude = Convert.ToDecimal(region.Longitude)
                };
            }
        }
        
        throw new ArgumentException($"Location '{ location.CloudProvider }' cannot be converted to Geoposition. ");
;
    }

}