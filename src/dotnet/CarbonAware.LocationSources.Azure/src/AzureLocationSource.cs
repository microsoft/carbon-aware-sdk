using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Azure.Model;

namespace CarbonAware.LocationSources.Azure;

/// <summary>
/// Reprsents an azure location source.
/// </summary>
public class AzureLocationSource : ILocationSource
{
    public string Name => "Azure Location Source";

    public string Description => "Location source that knows how to get and work with Azure location information.";

    private readonly ILogger<AzureLocationSource> _logger;

    private Dictionary<string, AzureRegion> azureRegions;

    /// <summary>
    /// Creates a new instance of the <see cref="AzureLocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public AzureLocationSource(ILogger<AzureLocationSource> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        azureRegions = GetAzureRegions();
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
        AzureRegion regionJson = azureRegions[regionName];
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

    protected virtual Dictionary<string, AzureRegion> GetAzureRegions()
    {
        var data = ReadFromResource("azure-regions.json");
        List<AzureRegion> regionList = JsonConvert.DeserializeObject<List<AzureRegion>>(data);
        foreach(AzureRegion regionInfo in regionList)
        {
            Console.Write("region");
            Console.WriteLine(regionInfo);
        }    
        // Create mapping
        var azureRegionsMapping = new Dictionary<string, AzureRegion>();
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
}