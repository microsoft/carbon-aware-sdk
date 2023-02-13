using CarbonAware.DataSources.Co2Signal.Client;
using CarbonAware.DataSources.Co2Signal.Model;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarbonAware.DataSources.Co2Signal.Tests;

[TestFixture]
public class Co2SignalDataSourceTests
{
    private Mock<ILogger<Co2SignalDataSource>> _logger { get; set; }

    private Mock<ICo2SignalClient> _Co2SignalClient { get; set; }

    private Co2SignalDataSource _dataSource { get; set; }

    private Mock<ILocationSource> _locationSource { get; set; }
    private static Location _defaultLocation = new Location() { Name = "eastus", Latitude = 34.123m, Longitude = 123.456m };
    private static string _defaultLatitude => _defaultLocation.Latitude.ToString() ?? "";
    private static string _defaultLongitude => _defaultLocation.Longitude.ToString() ?? "";
    private static DateTimeOffset _defaultDataStartTime = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<Co2SignalDataSource>>();
        _Co2SignalClient = new Mock<ICo2SignalClient>();
        _locationSource = new Mock<ILocationSource>();
        _dataSource = new Co2SignalDataSource(_logger.Object, _Co2SignalClient.Object, _locationSource.Object);
    }

    [Test]
    public async Task GetCarbonIntensity_ReturnsResultsWhenRecordsFound()
    {
        var startDate = DateTimeOffset.UtcNow.AddHours(-10);
        var endDate = startDate.AddHours(1);
        var expectedCarbonIntensity = 100;

        _locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Returns(Task.FromResult(_defaultLocation));

        LatestCarbonIntensityData emissionData = new()
        {
            Data = new CarbonIntensity()
            {
                Value = expectedCarbonIntensity,
            }
        };

        this._Co2SignalClient.Setup(c => c.GetLatestCarbonIntensityAsync(
            _defaultLatitude,
            _defaultLongitude)
        ).ReturnsAsync(() => emissionData);

        var result = await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate);

        Assert.IsNotNull(result);
        Assert.That(result.Count(), Is.EqualTo(1));

        var first = result.First();
        Assert.IsNotNull(first);
        Assert.That(first.Rating, Is.EqualTo(expectedCarbonIntensity));
        Assert.That(first.Location, Is.EqualTo(_defaultLocation.Name));

        this._locationSource.Verify(l => l.ToGeopositionLocationAsync(_defaultLocation));
    }

    [Test]
    public void GetCarbonIntensity_ThrowsWhenRegionNotFound()
    {
        var startDate = new DateTimeOffset(2022, 4, 18, 12, 32, 42, TimeSpan.FromHours(-6));
        var endDate = startDate.AddMinutes(1);

        this._locationSource.Setup(l => l.ToGeopositionLocationAsync(_defaultLocation)).Throws<LocationConversionException>();

        Assert.ThrowsAsync<LocationConversionException>(async () => await this._dataSource.GetCarbonIntensityAsync(new List<Location>() { _defaultLocation }, startDate, endDate));
    }
}