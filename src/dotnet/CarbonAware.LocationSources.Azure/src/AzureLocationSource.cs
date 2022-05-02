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

    private List<NamedGeoposition>? namedGeopositions;

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

    private List<NamedGeoposition>? LoadRegionsFromJson()
    {
        var data = ReadFromResource("CarbonAware.LocationSources.Azure.azure-regions.json");
        List<NamedGeoposition>? regionList = JsonSerializer.Deserialize<List<NamedGeoposition>>(data, options);
       
        return regionList;
    }

    public Location GetGeopositionLocation(Location location)
    {
        if(location.LocationType == LocationType.CloudProvider && location.CloudProvider != CloudProvider.Azure) {
            throw new ArgumentException($"Incorrect Cloud provider region. Expected Azure but found '{ location.CloudProvider }'");
        }
        NamedGeoposition region = namedGeopositions.Where(l => l.RegionName.Equals(location.RegionName)).First();

        return new Location {
            RegionName = region.RegionName,
            Latitude = Convert.ToDecimal(region.Latitude),
            Longitude = Convert.ToDecimal(region.Longitude)
        };
    }

}