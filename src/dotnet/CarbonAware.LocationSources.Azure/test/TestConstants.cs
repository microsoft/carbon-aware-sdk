using CarbonAware.Model;

namespace CarbonAware.LocationSources.Azure.Test;

public static class Constants
{

    public static readonly NamedGeoposition EastUsRegion = new () {
                    RegionName = "eastus",
                    Latitude = "37.3719",
                    Longitude = "-79.8164"
                    };
    public static readonly NamedGeoposition WestUsRegion = new () {
                    RegionName = "westus",
                    Latitude = "37.783",
                    Longitude = "-122.417"
                };
    public static readonly NamedGeoposition NorthCentralRegion = new () {
                    RegionName = "northcentralus",
                    Latitude = "37.783",
                    Longitude = "-122.417"
                };

    // public static readonly Location EastUsAzureLocation = new ()  {
    //     LocationType = LocationType.Azure,
    //     RegionName = "eastus"
    // };
    // public static readonly Location WestUsAzureLocation = new () {
    //     LocationType = LocationType.Azure,
    //     RegionName = "westus"
    // };
    // public static readonly Location NorthCentralAzureLocation = new () {
    //     LocationType = LocationType.Azure,
    //     RegionName = "northcentralus"
    // };

    // public static readonly Location EastUsGeoLocation = new ()  {
    //     LocationType = LocationType.Geoposition,
    //     Latitude = new decimal(37.3719),
    //     Longitude = new decimal(-79.8164)
    // };
    // public static readonly Location WestUsGeoLocation = new () {
    //     LocationType = LocationType.Geoposition,
    //     Latitude = new decimal(37.783),
    //     Longitude = new decimal(-122.417)
    // };
    // public static readonly Location NorthCentralGeoLocation = new () {
    //     LocationType = LocationType.Geoposition,
    //     Latitude = new decimal(37.783),
    //     Longitude = new decimal(-122.417)
    // };

    
}
