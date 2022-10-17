using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.Tools.WattTimeClient;
using GSF.CarbonIntensity.Exceptions;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonIntensity.Tests;

[TestFixture]
public class ForecastHandlerTests
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<ForecastHandler>> Logger { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<ForecastHandler>>();
    }

    [TestCase("Sydney", "eastus", "2022-03-07T01:00:00", "2022-03-07T03:30:00", 5, TestName = "All fields provided")]
    [TestCase("Sydney", null, null, null, null, TestName = "No optional fields provided")]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithOutputData(string location1, string? location2, DateTimeOffset? start, DateTimeOffset? end, int? duration)
    {
        var data = new List<CarbonAware.Model.EmissionsForecast> {
            new CarbonAware.Model.EmissionsForecast {
                RequestedAt = DateTimeOffset.Now,
                GeneratedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1),
                ForecastData = Array.Empty<CarbonAware.Model.EmissionsData>(),
                OptimalDataPoints = Array.Empty<CarbonAware.Model.EmissionsData>(),
            }
        };

        var aggregator = SetupMockAggregator(data);
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        var locations = location2 != null ? new string[] { location1, location2 } : new string[] { location1 };
        var result = await handler.GetCurrentAsync(locations, start, end, duration);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithoutOutputData()
    {
        var aggregator = SetupMockAggregator(Array.Empty<CarbonAware.Model.EmissionsForecast>().ToList());
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        var result = await handler.GetCurrentAsync(new string[] { "eastus" }, DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1), 30);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetCurrentAsync_ThrowsException()
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new WattTimeClientException(""));
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        Assert.ThrowsAsync<CarbonIntensityException>(async () => await handler.GetCurrentAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    private static Mock<IForecastAggregator> SetupMockAggregator(IEnumerable<CarbonAware.Model.EmissionsForecast> data)
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }
}
