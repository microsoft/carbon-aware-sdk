using CarbonAware.Model;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Options;
using CarbonAware.LocationSources.Configuration;

namespace CarbonAware.LocationSources.Test;

public class LocationSourceTest
{   
    [Test]
    public async Task TestToGeopositionLocation_ValidLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "eastus"
        };

        var eastResult = await mockLocationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "westus"
        };

        var westResult = await mockLocationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

    // <summary>
    // If an Azure Location with no LocationType is passed, should fail.
    // </summary>
    [Test]
    public void TestToGeopositionLocation_LocationTypeNotProvided_ThrowsException()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location invalidLocation = new Location()
        {
            RegionName = "eastus"
        };
        Assert.ThrowsAsync<LocationConversionException>(async() =>
        {
            await mockLocationSource.ToGeopositionLocationAsync(invalidLocation);
        });
    }

    // <summary>
    // If an Azure Location with invalid region name is passed, should fail.
    // </summary>
    [Test]
    public void TestToGeopositionLocation_InvalidRegionName_ThrowsException()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location invalidLocation = new Location()
        {
            RegionName = "invalid location",
            LocationType = LocationType.CloudProvider,
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await mockLocationSource.ToGeopositionLocationAsync(invalidLocation);
        });
    }

    /// <summary>
    /// If a Location with type LocationType.Geoposition is passed in, function
    /// returns original Location.
    /// </summary>
    [Test]
    public async Task TestToGeopositionLocation_AlreadyGeopositionLocation()
    {
        var mockLocationSource = SetupMockLocationSource().Object;
        Location location = new Location {
            LocationType = LocationType.Geoposition
        };
        var result = await mockLocationSource.ToGeopositionLocationAsync(location);
        Assert.AreEqual(location, result);
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_With_MultiConfiguration()
    {
        var configuration = new LocationDataSourcesConfiguration() 
        {
            LocationDataSources = new List<LocationDataSource>()
            {
                new LocationDataSource()
                {
                    Prefix = "prefix1",
                    Delimiter = '-',
                    DataFileLocation = "azure-regions.json"
                },
                new LocationDataSource()
                {
                    Prefix = "prefix2",
                    Delimiter = '_',
                    DataFileLocation = "azure-regions.json"
                }
            }
        };
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "prefix1-eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "prefix2_westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_Without_ConfigurationData_LoadDefaults()
    {
        var configuration = new LocationDataSourcesConfiguration();
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }


   [Test]
    public void GeopositionLocation_InvalidLocation_With_Configuration()
    {
        var configuration = new LocationDataSourcesConfiguration() 
        {
            LocationDataSources = new List<LocationDataSource>()
            {
                new LocationDataSource()
                {
                    Prefix = "test",
                    Delimiter = '-',
                    DataFileLocation = "azure-regions.json"
                }
            }
        };
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "eastus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });

        inputLocation = new Location {
            LocationType = LocationType.CloudProvider,
            RegionName = "westus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });
    }
 
    private static Mock<LocationSource> SetupMockLocationSource() {
        var logger = Mock.Of<ILogger<LocationSource>>();
        var monitor = Mock.Of<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        var mockLocationSource = new Mock<LocationSource>(logger, monitor);
                
        mockLocationSource.Protected()
            .Setup<Task<Dictionary<string, NamedGeoposition>>>("LoadRegionsFromJsonAsync")
            .ReturnsAsync(GetTestDataRegions())
            .Verifiable();

        return mockLocationSource;
    }

    private static Dictionary<string, NamedGeoposition> GetTestDataRegions() {
        // All the tests above correspond to values in this mock data. If the mock values are changed, the tests need to be updated 
        return new Dictionary<string, NamedGeoposition>() {
            {"eastus", Constants.EastUsRegion },
            {"westus", Constants.WestUsRegion },
            {"northcentralus", Constants.NorthCentralRegion }
        };
    }

    private static void AssertLocationsEqual(Location expected, Location actual)
    {
        Assert.AreEqual(expected.LocationType, actual.LocationType);
        Assert.AreEqual(expected.Latitude, actual.Latitude);
        Assert.AreEqual(expected.Longitude, actual.Longitude);
    }
}
