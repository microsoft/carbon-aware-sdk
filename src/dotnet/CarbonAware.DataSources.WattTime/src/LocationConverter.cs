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

}
