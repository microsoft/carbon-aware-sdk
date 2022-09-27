using CarbonAware.Model;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Options;
using CarbonAware.LocationSources.Configuration;

namespace CarbonAware.LocationSources.Test;

public class LocationSourceTest
{   
    [Test]
    public void GeopositionLocation_InvalidRegionName_ThrowsException()
    {
        var configuration = new LocationDataSourcesConfiguration();
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location invalidLocation = new Location()
        {
            Name = "invalid location"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(invalidLocation);
        });
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_With_MultiConfiguration()
    {
        var configuration = new LocationDataSourcesConfiguration();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
            {
                Prefix = "prefix1",
                Delimiter = "-",
                DataFileLocation = "azure-regions.json"
            });
        configuration.LocationSourceFiles.Add(new LocationSourceFile
            {
                Prefix = "prefix2",
                Delimiter = "_",
                DataFileLocation = "azure-regions.json"
            });
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = "prefix1-eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            Name = "prefix2_westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

    [Test]
    public async Task GeopositionLocation_ValidLocation_Without_Configuration_LoadDefaults()
    {
        var configuration = new LocationDataSourcesConfiguration();
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = "eastus"
        };

        var eastResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationEastUs, eastResult);

        inputLocation = new Location {
            Name = "westus"
        };

        var westResult = await locationSource.ToGeopositionLocationAsync(inputLocation);
        AssertLocationsEqual(Constants.LocationWestUs, westResult);
    }

   [Test]
    public void GeopositionLocation_InvalidLocation_With_Configuration()
    {
        var configuration = new LocationDataSourcesConfiguration();
        configuration.LocationSourceFiles.Add(new LocationSourceFile
        {
            Prefix = "test",
            Delimiter = "-",
            DataFileLocation = "azure-regions.json"
        });
        var options = new Mock<IOptionsMonitor<LocationDataSourcesConfiguration>>();
        options.Setup(o => o.CurrentValue).Returns(() => configuration);
        var logger = Mock.Of<ILogger<LocationSource>>();
        var locationSource = new LocationSource(logger, options.Object);

        Location inputLocation = new Location {
            Name = "eastus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });

        inputLocation = new Location {
            Name = "westus"
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await locationSource.ToGeopositionLocationAsync(inputLocation);
        });
    }
 
    private static void AssertLocationsEqual(Location expected, Location actual)
    {
        Assert.AreEqual(expected.Latitude, actual.Latitude);
        Assert.AreEqual(expected.Longitude, actual.Longitude);
    }
}
