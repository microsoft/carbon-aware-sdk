using CarbonAware.Model;
using CarbonAware.LocationSources.Azure.Model;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace CarbonAware.LocationSources.Azure.Test;

public class AzureLocationSourceTest
{   
    [Test]
    public async Task TestToGeoposition()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        
        var eastResult = await mockLocationSource.ToGeopositionLocation(Constants.EastUsAzureLocation);
        var westResult = await mockLocationSource.ToGeopositionLocation(Constants.WestUsAzureLocation);
        var northcentralResult = await mockLocationSource.ToGeopositionLocation(Constants.NorthCentralAzureLocation);

        AssertLocationsEqual(Constants.EastUsGeoLocation, eastResult);
        AssertLocationsEqual(Constants.WestUsGeoLocation, westResult);
        AssertLocationsEqual(Constants.NorthCentralGeoLocation, northcentralResult);
    }

    /// <summary>
    /// If an Azure Location with invalid RegionName is passed, should fail.
    /// </summary>
    [Test]
    public void TestToGeopositionInvalidLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location invalidLocation = new()
        {
            LocationType = LocationType.Azure,
            RegionName = "invalid location"
        };
        Assert.Throws<ArgumentException>(() =>
        {
            Task<Location> result = mockLocationSource.ToGeopositionLocation(invalidLocation);
        });
    }

    /// <summary>
    /// If a Location with type LocationType.Geoposition is passed in, function
    /// returns original Location.
    /// </summary>
    [Test]
    public async Task TestToGeopositionWhenAlreadyGeopositionLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        var result = await mockLocationSource.ToGeopositionLocation(Constants.EastUsGeoLocation);
        Assert.AreEqual(Constants.EastUsGeoLocation, result);
    }

    private static Mock<AzureLocationSource> SetupMockLocationSource() {
        var logger = Mock.Of<ILogger<AzureLocationSource>>();
        var mockLocationSource = new Mock<AzureLocationSource>(logger);
        
        mockLocationSource.Protected()
            .Setup<Dictionary<string, AzureRegion>>("GetAzureRegions")
            .Returns(GetTestAzureRegions())
            .Verifiable();

        return mockLocationSource;
    }

    private static Dictionary<string, AzureRegion> GetTestAzureRegions() {
        // All the tests above correspond to values in this mock data. If the mock values are changed, the tests need to be updated 
        return new Dictionary<string, AzureRegion>() {
            {"eastus", Constants.EastUsRegion },
            {"westus", Constants.WestUsRegion },
            {"northcentralus", Constants.NorthCentralRegion }
        };
    }

    private static void AssertLocationsEqual(Location location1, Location location2)
    {
        Assert.AreEqual(location1.RegionName, location2.RegionName);
        Assert.AreEqual(location1.LocationType, location2.LocationType);
        Assert.AreEqual(location1.Latitude, location2.Latitude);
        Assert.AreEqual(location1.Longitude, location2.Longitude);
    }
}
