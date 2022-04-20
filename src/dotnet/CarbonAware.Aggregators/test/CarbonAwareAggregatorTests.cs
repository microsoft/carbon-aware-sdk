using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using CarbonAware.Plugins;
using Microsoft.Extensions.Logging;
using System.Linq;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Aggregators.Configuration;

namespace CarbonAware.Aggregators.Tests;

[TestFixture]
public class CarbonAwareAggregatorTests
{

    [TestCase("westus", "2021-11-17", 10, ExpectedResult = 25)]
    [TestCase("eastus", "2021-11-17", 10, ExpectedResult = 60)]
    [TestCase("westus", "2021-11-19", 10, ExpectedResult = 0)]
    [TestCase("eastus", "2021-11-19", 10, ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18", 10, ExpectedResult = 0)]
    public async Task<double> Test_Emissions_Average_FakeData(string location, string startTime, int durationMinutes)
    {
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockPlugin = new Mock<ICarbonAware>();

        DateTime sTime;
        Assert.True(DateTime.TryParse(startTime, out sTime));

        mockPlugin.Setup(x => x.GetEmissionsDataAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(FilterRawFakeData(location, sTime));
        
        var aggregator = new CarbonAwareAggregator(logger, mockPlugin.Object);
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<string>() { location }},
            { CarbonAwareConstants.Start, sTime },
            { CarbonAwareConstants.Duration, durationMinutes }
        };
        return await aggregator.CalcEmissionsAverageAsync(props);
    }

    [TestCase("westus", "2021-11-17", 10, 20)]
    [TestCase("eastus", "2021-12-19", 10, 20)]
    [TestCase("fake", "2021-12-19", 10, 0)]
    public async Task Test_With_Plugin_Association(string location, string startTime, int durationMinutes, int expected)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); 
        serviceCollection.AddCarbonAwareEmissionServices();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var aggregators = serviceProvider.GetServices<ICarbonAwareAggregator>();
        Assert.NotNull(aggregators);
        Assert.IsNotEmpty(aggregators);

        DateTime sTime;
        Assert.True(DateTime.TryParse(startTime, out sTime));
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<string>() { location } },
            { CarbonAwareConstants.Start, sTime },
            { CarbonAwareConstants.Duration, durationMinutes }
        };
        var aggregator = aggregators.First();

        var average = await aggregator.CalcEmissionsAverageAsync(props);
        Assert.GreaterOrEqual(average, expected);
    }

    private IEnumerable<EmissionsData> FilterRawFakeData(string location, DateTime startTime)
    {
        return RawFakeEmissionData.Where(x => x.Location == location && x.Time >= startTime);
    }

    static IEnumerable<EmissionsData> RawFakeEmissionData =  new List<EmissionsData>()
        {
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17T04:45:11.5104572+00:00"),
                Rating = 10
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17T12:45:11.5104574+00:00"),
                Rating = 20
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-17T20:45:11.5104575+00:00"),
                Rating = 30
            },
            new EmissionsData {
                Location = "westus",
                Time = DateTime.Parse("2021-11-18T04:45:11.5104575+00:00"),
                Rating = 40
            },
            new EmissionsData {
                Location = "eastus",
                Time = DateTime.Parse("2021-11-18T07:06:11.510457+00:00"),
                Rating = 60
            }
        };
}
