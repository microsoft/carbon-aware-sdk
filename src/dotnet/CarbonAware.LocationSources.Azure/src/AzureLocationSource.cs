using System.Globalization;
using System.Reflection;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarbonAware.LocationSources.Azure;

/// <summary>
/// Reprsents an azure location source.
/// </summary>
public class AzureLocationSource : ILocationSource
{
    public string Name => "Azure Location Source";

    public string Description => "Location source that knows how to get and work with Azure location information.";

    private readonly ILogger<AzureLocationSource> _logger;

    private Dictionary<string, DataRegion> azureRegions;

    private Dictionary<string, RegionMetadata> regionCoordinates;

    /// <summary>
    /// Creates a new instance of the <see cref="AzureLocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public AzureLocationSource(ILogger<AzureLocationSource> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        azureRegions = ParseRegionsFromJson();
    }

    public Task<Location> ToGeopositionLocation(Location location)
    {
        if (location.LocationType == LocationType.Geoposition)
        {
            _logger.LogInformation("Provided location is already of type Geoposition. Returning provided location.");
            return Task.FromResult(location);
        }

        string regionName = location.RegionName;
        _logger.LogInformation("Converting Azure Location named {regionName} to Geoposition Location.", regionName);

        if (!azureRegions.ContainsKey(regionName))
        {
            Exception ex = new ArgumentException("Region name {regionName} not found in Azure location registry.", regionName);
            _logger.LogError("argument exception for region name", ex);
            throw ex;
        }
        DataRegion regionJson = azureRegions[regionName];
        decimal? latitude = decimal.Parse(regionJson.Metadata.Latitude, CultureInfo.InvariantCulture);
        decimal? longitude = decimal.Parse(regionJson.Metadata.Longitude, CultureInfo.InvariantCulture);

        Location updatedLocation = new ()
        {
            RegionName = null,
            LocationType = LocationType.Geoposition,
            Latitude = latitude,
            Longitude = longitude,
        };

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Converted Azure Location named '{regionName}' to Geoposition Location at latitude '{latitude}' and logitude '{longitude}'.", regionName, latitude, longitude);
        }

        return Task.FromResult(updatedLocation);

    }

    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }

    protected virtual Dictionary<string, DataRegion> ParseRegionsFromJson()
    {
        var data = ReadFromResource("azure-regions.json");
        List<DataRegion> regionList = JsonConvert.DeserializeObject<List<DataRegion>>(data);
        foreach(DataRegion regionInfo in regionList)
        {
            Console.Write("region");
            Console.WriteLine(regionInfo);
        }    
        // Create mapping
        var azureRegionsMapping = new Dictionary<string, DataRegion>();
        foreach (var region in regionList)
        {
            azureRegionsMapping[region.Name] = region;
        }

        // Cache mapping if didn't exist
        if(!azureRegions.Any()) {
           azureRegions = azureRegionsMapping;
        }

        return azureRegionsMapping;
    }

    public Dictionary<string, RegionMetadata> GetRegionCordinates()
    {
        return regionCoordinates;
    }
}