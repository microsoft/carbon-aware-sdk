using CarbonAware.LocationSources.Exceptions;
using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[TestFixture]
public class ElectricityMapsDataSourceTests
{
    private Mock<ILogger<ElectricityMapsDataSource>> _logger { get; set; }

    private Mock<IElectricityMapsClient> _electricityMapsClient { get; set; }

    private ElectricityMapsDataSource _dataSource { get; set; }

    private Mock<ILocationSource> _locationSource { get; set; }
    private Location _defaultLocation { get; set; }
    private string _defaultLatitude => _defaultLocation.Latitude.ToString() ?? "";
    private string _defaultLongitude => _defaultLocation.Longitude.ToString() ?? "";
    private DateTimeOffset _defaultDataStartTime { get; set; }

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<ElectricityMapsDataSource>>();
        _electricityMapsClient = new Mock<IElectricityMapsClient>();
        _locationSource = new Mock<ILocationSource>();
        _dataSource = new ElectricityMapsDataSource(_logger.Object, _electricityMapsClient.Object, _locationSource.Object);
        _defaultLocation = new Location() { Name = "eastus", Latitude = 34.123m, Longitude = 123.456m };
        _defaultDataStartTime = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
    }

    [Test]
    public async Task GetCurrentCarbonIntensityForecastAsync_ReturnsResultsWhenRecordsFound()
    {
        // Arrange
        var startDate = _defaultDataStartTime;
        var endDate = startDate.AddMinutes(1);
        var updatedAt = new DateTimeOffset(2022, 4, 18, 12, 30, 00, TimeSpan.FromHours(-6));
        var expectedDuration = TimeSpan.FromMinutes(5);
        var expectedCarbonIntensity = 10;

        var forecast = new ForecastedCarbonIntensityData()
        {
            UpdatedAt = updatedAt,
            ForecastData = new List<Forecast>()
            {
                new Forecast()
                {
                    CarbonIntensity = expectedCarbonIntensity,
                    DateTime = startDate
                },
                new Forecast()
                {
                    CarbonIntensity = expectedCarbonIntensity,
                    DateTime = startDate + expectedDuration
                },
            }
        };

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));
        _electricityMapsClient.Setup(client => client.GetCurrentForecastAsync(_defaultLatitude, _defaultLongitude)
            ).ReturnsAsync(() => forecast);

        // Act
        var result = await _dataSource.GetCurrentCarbonIntensityForecastAsync(_defaultLocation);

        // Assert
        Assert.IsNotNull(result);
        Assert.That(result.GeneratedAt, Is.EqualTo(updatedAt));
        Assert.That(result.Location, Is.EqualTo(_defaultLocation));

        var firstDataPoint = result.ForecastData.First();
        var lastDataPoint = result.ForecastData.Last();
        Assert.IsNotNull(firstDataPoint);
        Assert.That(firstDataPoint.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(firstDataPoint.Location, Is.EqualTo(_defaultLocation.Name));
        Assert.That(firstDataPoint.Time, Is.EqualTo(startDate));
        Assert.That(firstDataPoint.Duration, Is.EqualTo(expectedDuration));

        Assert.IsNotNull(lastDataPoint);
        Assert.That(lastDataPoint.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(lastDataPoint.Location, Is.EqualTo(_defaultLocation.Name));
        Assert.That(lastDataPoint.Time, Is.EqualTo(startDate + expectedDuration));
        Assert.That(lastDataPoint.Duration, Is.EqualTo(expectedDuration));

        _locationSource.Verify(r => r.ToGeopositionLocationAsync(_defaultLocation), Times.Once);
    }

    [Test]
    public void GetCurrentCarbonIntensityForecastAsync_ThrowsWhenRegionNotFound()
    {
        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await _dataSource.GetCurrentCarbonIntensityForecastAsync(_defaultLocation));
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
