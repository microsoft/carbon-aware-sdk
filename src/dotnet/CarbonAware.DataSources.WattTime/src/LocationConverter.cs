using System.Reflection;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarbonAware.DataSources.WattTime;

/// <inheritdoc />
public class LocationConverter : ILocationConverter
{
    /// <inheritdoc />
    private ILogger<LocationConverter> Logger { get; }
    private ILocationSource LocationSource {get; }

    //Create constrctor
    public LocationConverter(ILogger<LocationConverter> logger, ILocationSource locationSource ) {
        this.Logger = logger;
        this.LocationSource = locationSource;        
    }
    public async Task<RegionMetadata> ConvertLocationToLatLongAsync(Location location)
    {
        switch (location.LocationType)
        {
            case LocationType.Geoposition:
            {
                    return new RegionMetadata {
                        Latitude = location.Latitude.ToString(),
                        Longitude = location.Longitude.ToString()
                    };
            }
            case LocationType.Azure:
            {
                    return LocationSource.GetRegionCordinates()[location.RegionName];

            }
        }
        throw new LocationConversionException();        
         
    }


    private void read() {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream("") ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        var data = readerMetaData.ReadToEnd();

        JObject googleSearch = JObject.Parse(data);

        IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

        // // serialize JSON results into .NET objects
        // IList<SearchResult> searchResults = new List<SearchResult>();
        // foreach (JToken result in results)
        // {
        //     // JToken.ToObject is a helper method that uses JsonSerializer internally
        //     SearchResult searchResult = result.ToObject<SearchResult>();
        //     searchResults.Add(searchResult);
        // }
    }
}
